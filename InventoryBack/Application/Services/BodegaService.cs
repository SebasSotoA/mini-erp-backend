using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class BodegaService : IBodegaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BodegaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BodegaDto> CreateAsync(CreateBodegaDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingBodega = await _unitOfWork.Bodegas.GetByNameAsync(dto.Nombre, ct);
        if (existingBodega != null)
        {
            throw new BusinessRuleException($"Ya existe una bodega con el nombre '{dto.Nombre}'.");
        }

        var bodega = new Bodega
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Bodegas.AddAsync(bodega, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<BodegaDto>(bodega);
    }

    public async Task<BodegaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        return bodega != null ? _mapper.Map<BodegaDto>(bodega) : null;
    }

    public async Task<IEnumerable<BodegaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<Bodega> bodegas;

        if (activo.HasValue)
        {
            bodegas = await _unitOfWork.Bodegas.ListAsync(
                filter: b => b.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            bodegas = await _unitOfWork.Bodegas.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<BodegaDto>>(bodegas);
    }

    public async Task<PagedResult<BodegaDto>> GetPagedAsync(BodegaFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Bodegas.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<BodegaDto>>(items);

        return new PagedResult<BodegaDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        // Validate name uniqueness (excluding current bodega)
        var existingBodega = await _unitOfWork.Bodegas.GetByNameAsync(dto.Nombre, ct);
        if (existingBodega != null && existingBodega.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otra bodega con el nombre '{dto.Nombre}'.");
        }

        bodega.Nombre = dto.Nombre.Trim();
        bodega.Direccion = dto.Direccion?.Trim();
        bodega.Descripcion = dto.Descripcion?.Trim();

        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        if (bodega.Activo)
        {
            throw new BusinessRuleException("La bodega ya está activa.");
        }

        bodega.Activo = true;
        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        if (!bodega.Activo)
        {
            throw new BusinessRuleException("La bodega ya está desactivada.");
        }

        // Check if bodega has products
        var hasProducts = await _unitOfWork.Bodegas.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede desactivar la bodega porque tiene productos asignados. " +
                "Reasigne los productos a otra bodega antes de desactivarla.");
        }

        bodega.Activo = false;
        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        // Check if bodega has products
        var hasProducts = await _unitOfWork.Bodegas.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene productos asignados. " +
                "Elimine o reasigne todos los productos antes de eliminar la bodega.");
        }

        // Check if bodega has invoices
        var hasInvoices = await _unitOfWork.Bodegas.HasInvoicesAsync(id, ct);
        if (hasInvoices)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene facturas asociadas. " +
                "Considere desactivarla en su lugar.");
        }

        // Check if bodega has inventory movements
        var hasMovements = await _unitOfWork.Bodegas.HasMovementsAsync(id, ct);
        if (hasMovements)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene movimientos de inventario registrados. " +
                "Considere desactivarla en su lugar.");
        }

        _unitOfWork.Bodegas.Remove(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProductInBodegaDto>> GetProductsInBodegaAsync(
        Guid bodegaId, 
        ProductFilterDto filters, 
        CancellationToken ct = default)
    {
        // Validate bodega exists
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(bodegaId, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", bodegaId);
        }

        // Get all ProductoBodega entries for this warehouse
        var productoBodegas = await _unitOfWork.ProductoBodegas.GetByBodegaIdAsync(bodegaId, ct);
        
        // Get product IDs
        var productIds = productoBodegas.Select(pb => pb.ProductoId).ToHashSet();

        // If no products in this bodega, return empty result
        if (!productIds.Any())
        {
            return new PagedResult<ProductInBodegaDto>
            {
                Items = new List<ProductInBodegaDto>(),
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = 0
            };
        }

        // Get all products that are in this bodega
        var allProducts = await _unitOfWork.Products.ListAsync(
            filter: p => productIds.Contains(p.Id),
            ct: ct
        );

        // Apply filters manually (same logic as ProductRepository)
        var filteredProducts = allProducts.AsQueryable();

        // Apply product filters (name, SKU, description, status)
        filteredProducts = ApplyProductFilters(filteredProducts, filters);

        // Apply price filter (separate method for clarity)
        filteredProducts = ApplyPriceFilter(filteredProducts, filters);

        // Apply quantity filters (specific to this bodega)
        filteredProducts = ApplyQuantityFiltersInBodega(filteredProducts, bodegaId, filters);

        // Get total count before pagination
        var totalCount = filteredProducts.Count();

        // Apply ordering
        filteredProducts = ApplyProductOrdering(filteredProducts, filters.OrderBy, filters.OrderDesc);

        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        // Apply pagination
        var paginatedProducts = filteredProducts
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        // Map to DTOs
        var productDtos = new List<ProductInBodegaDto>();

        // Create a dictionary for fast lookup of quantities in this bodega
        var bodegaStockLookup = productoBodegas.ToDictionary(
            pb => pb.ProductoId,
            pb => pb.StockActual
        );

        foreach (var producto in paginatedProducts)
        {
            // Map base properties
            var productDto = _mapper.Map<ProductInBodegaDto>(producto);
            
            // Calculate total stock across all warehouses for this product
            productDto.StockActual = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);

            // ? NEW: Set quantity in THIS specific bodega
            productDto.CantidadEnBodega = bodegaStockLookup.TryGetValue(producto.Id, out var cantidad) 
                ? cantidad 
                : 0;

            // Load category name if product has a category
            if (producto.CategoriaId.HasValue)
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(producto.CategoriaId.Value, ct);
                productDto.CategoriaNombre = categoria?.Nombre;
            }

            productDtos.Add(productDto);
        }

        return new PagedResult<ProductInBodegaDto>
        {
            Items = productDtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    // ========== PRIVATE HELPER METHODS FOR PRODUCT FILTERING ==========

    private IQueryable<Producto> ApplyProductFilters(IQueryable<Producto> query, ProductFilterDto filters)
    {
        // Global search (nombre, SKU, descripcion)
        if (!string.IsNullOrWhiteSpace(filters.Q))
        {
            var searchTerm = filters.Q.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(searchTerm) ||
                (p.CodigoSku != null && p.CodigoSku.ToLower().Contains(searchTerm)) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(searchTerm))
            );
        }

        // Filter by nombre
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

        // Filter by descripcion
        if (!string.IsNullOrWhiteSpace(filters.Descripcion))
        {
            var desc = filters.Descripcion.Trim().ToLower();
            query = query.Where(p => p.Descripcion != null && p.Descripcion.ToLower().Contains(desc));
        }

        // Filter by active status
        if (filters.IncludeInactive)
        {
            // Include all products (both active and inactive)
        }
        else if (filters.OnlyInactive)
        {
            query = query.Where(p => !p.Activo);
        }
        else
        {
            // Default: only active products
            query = query.Where(p => p.Activo);
        }

        return query;
    }

    private IQueryable<Producto> ApplyPriceFilter(IQueryable<Producto> query, ProductFilterDto filters)
    {
        // Price search (partial match - searches as text)
        // This allows searching for "1500" to find prices like 1500000, 2500000, 150000, etc.
        if (!string.IsNullOrWhiteSpace(filters.Precio))
        {
            var precioSearch = filters.Precio.Trim();
            
            // Note: Since we're working with IQueryable<Producto> in memory (already loaded via ListAsync),
            // we need to filter by converting to string for partial matches.
            // For better performance in large datasets, consider filtering by decimal ranges instead.
            query = query.Where(p => p.PrecioBase.ToString().Contains(precioSearch));
        }

        return query;
    }

    private IQueryable<Producto> ApplyQuantityFiltersInBodega(
        IQueryable<Producto> query, 
        Guid bodegaId, 
        ProductFilterDto filters)
    {
        // If no quantity filters, return as is
        if (!filters.CantidadExacta.HasValue &&
            !filters.CantidadMin.HasValue &&
            !filters.CantidadMax.HasValue)
        {
            return query;
        }

        // Get ProductoBodega entries for this specific bodega only
        var productoBodegaQuery = _unitOfWork.ProductoBodegas
            .ListAsync(filter: pb => pb.BodegaId == bodegaId)
            .Result
            .AsQueryable();

        // Exact quantity
        if (filters.CantidadExacta.HasValue)
        {
            var productIds = productoBodegaQuery
                .Where(pb => pb.StockActual == filters.CantidadExacta.Value)
                .Select(pb => pb.ProductoId)
                .Distinct()
                .ToList();

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
                    .Distinct()
                    .ToList();

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
                        .Distinct()
                        .ToList(),
                    ">=" => productoBodegaQuery
                        .Where(pb => pb.StockActual >= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                        .ToList(),
                    "=" => productoBodegaQuery
                        .Where(pb => pb.StockActual == filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                        .ToList(),
                    "<" => productoBodegaQuery
                        .Where(pb => pb.StockActual < filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                        .ToList(),
                    "<=" => productoBodegaQuery
                        .Where(pb => pb.StockActual <= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                        .ToList(),
                    _ => productoBodegaQuery
                        .Where(pb => pb.StockActual >= filters.CantidadMin.Value)
                        .Select(pb => pb.ProductoId)
                        .Distinct()
                        .ToList()
                };

                return query.Where(p => productIds.Contains(p.Id));
            }
            else if (filters.CantidadMax.HasValue)
            {
                // Only max specified
                var productIds = productoBodegaQuery
                    .Where(pb => pb.StockActual <= filters.CantidadMax.Value)
                    .Select(pb => pb.ProductoId)
                    .Distinct()
                    .ToList();

                return query.Where(p => productIds.Contains(p.Id));
            }
        }

        return query;
    }

    private IQueryable<Producto> ApplyProductOrdering(
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

            "costo" => orderDesc
                ? query.OrderByDescending(p => p.CostoInicial)
                : query.OrderBy(p => p.CostoInicial),

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
