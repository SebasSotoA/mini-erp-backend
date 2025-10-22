using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with specific queries.
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
        int page,
        int pageSize,
        string? searchQuery = null,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        IQueryable<Producto> query = _dbSet.AsNoTracking();

        // Filter by active status
        if (!includeInactive)
        {
            query = query.Where(p => p.Activo);
        }

        // Search in Name, SKU, and Description
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(search) ||
                (p.CodigoSku != null && p.CodigoSku.ToLower().Contains(search)) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(search))
            );
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
}
