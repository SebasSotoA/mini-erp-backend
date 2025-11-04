using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with specific queries and advanced filtering.
/// </summary>
public class ProductRepository : EfGenericRepository<Producto>, IProductRepository
{
    public ProductRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<Producto?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodigoSku == sku, ct);
    }

    public async Task<(IEnumerable<Producto> Items, int TotalCount)> GetPagedAsync(
        ProductFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<Producto> query = _dbSet.AsNoTracking();

        // ========== 1. STATUS FILTER ==========
        query = ApplyStatusFilter(query, filters);

        // ========== 2. GENERAL SEARCH (Q) ==========
        query = ApplyGeneralSearch(query, filters.Q);

        // ========== 3. SPECIFIC FILTERS ==========
        query = ApplySpecificFilters(query, filters);

        // ========== 4. PRICE FILTERS ==========
        query = ApplyPriceFilters(query, filters);

        // ========== 5. QUANTITY FILTERS ==========
        // Note: Requires join with ProductoBodega
        query = ApplyQuantityFilters(query, filters);

        // ========== 6. SORTING ==========
        query = ApplyOrdering(query, filters.OrderBy, filters.OrderDesc);

        // ========== 7. GET TOTAL COUNT ==========
        var totalCount = await query.CountAsync(ct);

        // ========== 8. APPLY PAGINATION ==========
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var query = _dbSet.Where(p => p.CodigoSku == sku);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> IsProductReferencedAsync(Guid productId, CancellationToken ct = default)
    {
        // Check if product is referenced in any sale or purchase invoice details
        var isReferencedInSales = await _db.FacturasVentaDetalle
            .AnyAsync(fvd => fvd.ProductoId == productId, ct);

        if (isReferencedInSales)
            return true;

        var isReferencedInPurchases = await _db.FacturasCompraDetalle
            .AnyAsync(fcd => fcd.ProductoId == productId, ct);

        return isReferencedInPurchases;
    }

    public async Task<bool> HasExtraFieldsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.ProductoCamposExtra
            .AnyAsync(pce => pce.ProductoId == productId, ct);
    }

    public async Task<bool> HasInventoryMovementsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.MovimientosInventario
            .AnyAsync(mi => mi.ProductoId == productId, ct);
    }

    public async Task<Dictionary<string, int>> GetProductDependenciesAsync(Guid productId, CancellationToken ct = default)
    {
        var dependencies = new Dictionary<string, int>();

        // Count warehouse relationships (ProductoBodegas)
        dependencies["bodegas"] = await _db.ProductoBodegas
            .CountAsync(pb => pb.ProductoId == productId, ct);

        // Count extra fields
        dependencies["camposExtra"] = await _db.ProductoCamposExtra
            .CountAsync(pce => pce.ProductoId == productId, ct);

        // Count sale invoice details
        dependencies["facturasVenta"] = await _db.FacturasVentaDetalle
            .CountAsync(fvd => fvd.ProductoId == productId, ct);

        // Count purchase invoice details
        dependencies["facturasCompra"] = await _db.FacturasCompraDetalle
            .CountAsync(fcd => fcd.ProductoId == productId, ct);

        // Count inventory movements
        dependencies["movimientosInventario"] = await _db.MovimientosInventario
            .CountAsync(mi => mi.ProductoId == productId, ct);

        return dependencies;
    }

    public async Task<IEnumerable<ProductoBodega>> GetProductWarehousesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.ProductoBodegas
            .AsNoTracking()
            .Where(pb => pb.ProductoId == productId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProductoCampoExtra>> GetProductExtraFieldsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.ProductoCamposExtra
            .AsNoTracking()
            .Where(pce => pce.ProductoId == productId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets the total stock for a product across all warehouses.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sum of StockActual from all ProductoBodega records</returns>
    public async Task<int> GetTotalStockAsync(Guid productId, CancellationToken ct = default)
    {
        var totalStock = await _db.ProductoBodegas
            .Where(pb => pb.ProductoId == productId)
            .SumAsync(pb => pb.StockActual, ct);

        return totalStock;
    }

    /// <summary>
    /// Gets total stock for multiple products at once (batch operation).
    /// </summary>
    /// <param name="productIds">List of product IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dictionary mapping ProductId to total stock</returns>
    public async Task<Dictionary<Guid, int>> GetTotalStockBatchAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        var stocks = await _db.ProductoBodegas
            .Where(pb => productIds.Contains(pb.ProductoId))
            .GroupBy(pb => pb.ProductoId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(pb => pb.StockActual) })
            .ToListAsync(ct);

        return stocks.ToDictionary(x => x.ProductId, x => x.TotalStock);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private IQueryable<Producto> ApplyStatusFilter(IQueryable<Producto> query, ProductFilterDto filters)
    {
        if (filters.OnlyInactive)
        {
            // Only inactive products
            return query.Where(p => !p.Activo);
        }
        else if (!filters.IncludeInactive)
        {
            // Only active products (default behavior)
            return query.Where(p => p.Activo);
        }

        // If includeInactive is true and onlyInactive is false, return all (no filter)
        return query;
    }

    private IQueryable<Producto> ApplyGeneralSearch(IQueryable<Producto> query, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return query;

        var search = searchQuery.Trim().ToLower();
        return query.Where(p =>
            p.Nombre.ToLower().Contains(search) ||
            (p.CodigoSku != null && p.CodigoSku.ToLower().Contains(search)) ||
            (p.Descripcion != null && p.Descripcion.ToLower().Contains(search))
        );
    }

    private IQueryable<Producto> ApplySpecificFilters(IQueryable<Producto> query, ProductFilterDto filters)
    {
        // Filter by Name
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(nombre));
        }

        // Filter by SKU
        if (!string.IsNullOrWhiteSpace(filters.CodigoSku))
        {
            var sku = filters.CodigoSku.Trim().ToLower();
            query = query.Where(p => p.CodigoSku != null && p.CodigoSku.ToLower().Contains(sku));
        }

        // Filter by Description
        if (!string.IsNullOrWhiteSpace(filters.Descripcion))
        {
            var desc = filters.Descripcion.Trim().ToLower();
            query = query.Where(p => p.Descripcion != null && p.Descripcion.ToLower().Contains(desc));
        }

        return query;
    }

    private IQueryable<Producto> ApplyPriceFilters(IQueryable<Producto> query, ProductFilterDto filters)
    {
        // Price search (partial match - searches as text)
        if (!string.IsNullOrWhiteSpace(filters.Precio))
        {
            var precioSearch = filters.Precio.Trim();
            
            // Convert price to string and search for partial matches
            // This allows searching for "1500" to find prices like 1500000, 2500000, 150000, etc.
            query = query.Where(p => 
                EF.Functions.Like(p.PrecioBase.ToString(), $"%{precioSearch}%")
            );
        }

        return query;
    }

    private IQueryable<Producto> ApplyQuantityFilters(IQueryable<Producto> query, ProductFilterDto filters)
    {
        // If no quantity filters, return as is
        if (!filters.CantidadExacta.HasValue &&
            !filters.CantidadMin.HasValue &&
            !filters.CantidadMax.HasValue)
        {
            return query;
        }

        // Join with ProductoBodega to filter by quantity
        var productoBodegaQuery = _db.ProductoBodegas.AsNoTracking();

        // Exact quantity
        if (filters.CantidadExacta.HasValue)
        {
            var productIds = productoBodegaQuery
                .Where(pb => pb.StockActual == filters.CantidadExacta.Value)
                .Select(pb => pb.ProductoId)
                .Distinct();

            return query.Where(p => productIds.Contains(p.Id));
        }

        // Quantity with operator
        if (filters.CantidadMin.HasValue || filters.CantidadMax.HasValue)
        {
            var operador = filters.CantidadOperador?.ToLower() ?? ">=";

            if (operador == "range" && filters.CantidadMin.HasValue && filters.CantidadMax.HasValue)
            {
                // Range filter
                var productIds = productoBodegaQuery
                    .Where(pb => pb.StockActual >= filters.CantidadMin.Value &&
                                 pb.StockActual <= filters.CantidadMax.Value)
                    .Select(pb => pb.ProductoId)
                    .Distinct();

                return query.Where(p => productIds.Contains(p.Id));
            }
            else if (filters.CantidadMin.HasValue)
            {
                // Apply operator with CantidadMin
                var productIds = operador switch
                {
                    ">" => productoBodegaQuery
                        .Where(pb => pb.StockActual > filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct(),
                    ">=" => productoBodegaQuery
                        .Where(pb => pb.StockActual >= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct(),
                    "=" => productoBodegaQuery
                        .Where(pb => pb.StockActual == filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct(),
                    "<" => productoBodegaQuery
                        .Where(pb => pb.StockActual < filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct(),
                    "<=" => productoBodegaQuery
                        .Where(pb => pb.StockActual <= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct(),
                    _ => productoBodegaQuery
                        .Where(pb => pb.StockActual >= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                };

                return query.Where(p => productIds.Contains(p.Id));
            }
            else if (filters.CantidadMax.HasValue)
            {
                // Only max specified
                var productIds = productoBodegaQuery
                    .Where(pb => pb.StockActual <= filters.CantidadMax.Value)
                    .Select(pb => pb.ProductoId)
                    .Distinct();

                return query.Where(p => productIds.Contains(p.Id));
            }
        }

        return query;
    }

    private IQueryable<Producto> ApplyOrdering(
        IQueryable<Producto> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "precio" => orderDesc
                ? query.OrderByDescending(p => p.PrecioBase)
                : query.OrderBy(p => p.PrecioBase),

            "sku" => orderDesc
                ? query.OrderByDescending(p => p.CodigoSku)
                : query.OrderBy(p => p.CodigoSku),

            "fecha" => orderDesc
                ? query.OrderByDescending(p => p.FechaCreacion)
                : query.OrderBy(p => p.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(p => p.Nombre)
                : query.OrderBy(p => p.Nombre)
        };
    }
}
