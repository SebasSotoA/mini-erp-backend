using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class CampoExtraService : ICampoExtraService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CampoExtraService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CampoExtraDto> CreateAsync(CreateCampoExtraDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingCampo = await _unitOfWork.CamposExtras.GetByNameAsync(dto.Nombre, ct);
        if (existingCampo != null)
        {
            throw new BusinessRuleException($"Ya existe un campo extra con el nombre '{dto.Nombre}'.");
        }

        var campoExtra = new CampoExtra
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            TipoDato = dto.TipoDato.Trim(),
            EsRequerido = dto.EsRequerido,
            ValorPorDefecto = dto.ValorPorDefecto?.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.CamposExtras.AddAsync(campoExtra, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CampoExtraDto>(campoExtra);
    }

    public async Task<CampoExtraDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        return campoExtra != null ? _mapper.Map<CampoExtraDto>(campoExtra) : null;
    }

    public async Task<IEnumerable<CampoExtraDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<CampoExtra> camposExtra;

        if (activo.HasValue)
        {
            camposExtra = await _unitOfWork.CamposExtras.ListAsync(
                filter: ce => ce.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            camposExtra = await _unitOfWork.CamposExtras.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<CampoExtraDto>>(camposExtra);
    }

    public async Task<PagedResult<CampoExtraDto>> GetPagedAsync(CampoExtraFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.CamposExtras.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<CampoExtraDto>>(items);

        return new PagedResult<CampoExtraDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateCampoExtraDto dto, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        // Validate name uniqueness (excluding current campo)
        var existingCampo = await _unitOfWork.CamposExtras.GetByNameAsync(dto.Nombre, ct);
        if (existingCampo != null && existingCampo.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otro campo extra con el nombre '{dto.Nombre}'.");
        }

        campoExtra.Nombre = dto.Nombre.Trim();
        campoExtra.TipoDato = dto.TipoDato.Trim();
        campoExtra.EsRequerido = dto.EsRequerido;
        campoExtra.ValorPorDefecto = dto.ValorPorDefecto?.Trim();
        campoExtra.Descripcion = dto.Descripcion?.Trim();

        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        if (campoExtra.Activo)
        {
            throw new BusinessRuleException("El campo extra ya está activo.");
        }

        campoExtra.Activo = true;
        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        if (!campoExtra.Activo)
        {
            throw new BusinessRuleException("El campo extra ya está desactivado.");
        }

        // ? VALIDACIÓN CONDICIONAL: Solo validar si el campo es requerido
        if (campoExtra.EsRequerido)
        {
            var productCount = await _unitOfWork.ProductoCamposExtras.CountAsync(
                filter: pce => pce.CampoExtraId == id,
                ct: ct);

            if (productCount > 0)
            {
                throw new BusinessRuleException(
                    $"No se puede desactivar el campo extra requerido '{campoExtra.Nombre}' " +
                    $"porque está asignado a {productCount} producto(s). " +
                    "Para desactivarlo debe:\n" +
                    "1. Cambiar 'EsRequerido' a false y luego desactivar, O\n" +
                    "2. Eliminar el campo de todos los productos antes de desactivar\n\n" +
                    "Los campos requeridos con productos asociados no pueden desactivarse para mantener la integridad de los datos.");
            }
        }

        campoExtra.Activo = false;
        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        // Check if campo is being used in products
        var isInUse = await _unitOfWork.ProductoCamposExtras.CountAsync(
            filter: pce => pce.CampoExtraId == id,
            ct: ct);

        if (isInUse > 0)
        {
            throw new BusinessRuleException(
                "No se puede eliminar el campo extra porque está siendo utilizado en productos. " +
                "Considere desactivarlo en su lugar.");
        }

        _unitOfWork.CamposExtras.Remove(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProductInCampoExtraDto>> GetProductsInCampoExtraAsync(
        Guid campoExtraId, 
        ProductFilterDto filters, 
        CancellationToken ct = default)
    {
        // Validate campo extra exists
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(campoExtraId, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", campoExtraId);
        }

        // Get all ProductoCampoExtra entries for this campo
        var productoCampos = await _unitOfWork.ProductoCamposExtras.ListAsync(
            filter: pce => pce.CampoExtraId == campoExtraId,
            ct: ct
        );

        // Get product IDs
        var productIds = productoCampos.Select(pc => pc.ProductoId).ToHashSet();

        // If no products have this campo extra, return empty result
        if (!productIds.Any())
        {
            return new PagedResult<ProductInCampoExtraDto>
            {
                Items = new List<ProductInCampoExtraDto>(),
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = 0
            };
        }

        // Get all products that have this campo extra assigned
        var allProducts = await _unitOfWork.Products.ListAsync(
            filter: p => productIds.Contains(p.Id),
            ct: ct
        );

        // Apply filters manually (same logic as ProductRepository)
        var filteredProducts = allProducts.AsQueryable();

        // Apply product filters (name, SKU, description, status)
        filteredProducts = ApplyProductFilters(filteredProducts, filters);

        // Apply price filter
        filteredProducts = ApplyPriceFilter(filteredProducts, filters);

        // Apply quantity filters (total stock across all warehouses)
        filteredProducts = ApplyQuantityFilters(filteredProducts, filters);

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

        // Create lookup dictionary for campo extra values
        var campoValoresLookup = productoCampos.ToDictionary(
            pc => pc.ProductoId,
            pc => pc.Valor
        );

        // Map to DTOs
        var productDtos = new List<ProductInCampoExtraDto>();

        foreach (var producto in paginatedProducts)
        {
            // Map base properties
            var productDto = _mapper.Map<ProductInCampoExtraDto>(producto);
            
            // Calculate total stock across all warehouses for this product
            var totalStock = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            productDto.StockActual = totalStock;

            // Set quantity in campo (same as total stock, included for consistency)
            productDto.CantidadEnCampo = totalStock;

            // Set the value of this campo extra for this product
            productDto.ValorCampoExtra = campoValoresLookup.TryGetValue(producto.Id, out var valor) 
                ? valor 
                : null;

            // Load category name if product has a category
            if (producto.CategoriaId.HasValue)
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(producto.CategoriaId.Value, ct);
                productDto.CategoriaNombre = categoria?.Nombre;
            }

            productDtos.Add(productDto);
        }

        return new PagedResult<ProductInCampoExtraDto>
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
        // Price search (partial match as text)
        if (!string.IsNullOrWhiteSpace(filters.Precio))
        {
            var precioText = filters.Precio.Trim();
            query = query.Where(p => p.PrecioBase.ToString().Contains(precioText));
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

        // Get ProductoBodega entries for total stock calculation
        var productoBodegaQuery = _unitOfWork.ProductoBodegas
            .ListAsync()
            .Result
            .AsQueryable();

        // Group by ProductId and sum StockActual
        var productStocks = productoBodegaQuery
            .GroupBy(pb => pb.ProductoId)
            .Select(g => new { ProductoId = g.Key, TotalStock = g.Sum(pb => pb.StockActual) })
            .ToList();

        // Exact quantity
        if (filters.CantidadExacta.HasValue)
        {
            var productIds = productStocks
                .Where(ps => ps.TotalStock == filters.CantidadExacta.Value)
                .Select(ps => ps.ProductoId)
                .ToList();

            return query.Where(p => productIds.Contains(p.Id));
        }

        // Quantity with operator
        if (filters.CantidadMin.HasValue || filters.CantidadMax.HasValue)
        {
            var operador = filters.CantidadOperador?.ToLower() ?? ">=";

            if (operador == "range" && filters.CantidadMin.HasValue && filters.CantidadMax.HasValue)
            {
                var productIds = productStocks
                    .Where(ps => ps.TotalStock >= filters.CantidadMin.Value &&
                                 ps.TotalStock <= filters.CantidadMax.Value)
                    .Select(ps => ps.ProductoId)
                    .ToList();

                return query.Where(p => productIds.Contains(p.Id));
            }
            else if (filters.CantidadMin.HasValue)
            {
                var productIds = operador switch
                {
                    ">" => productStocks
                        .Where(ps => ps.TotalStock > filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList(),
                    ">=" => productStocks
                        .Where(ps => ps.TotalStock >= filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList(),
                    "=" => productStocks
                        .Where(ps => ps.TotalStock == filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList(),
                    "<" => productStocks
                        .Where(ps => ps.TotalStock < filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList(),
                    "<=" => productStocks
                        .Where(ps => ps.TotalStock <= filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList(),
                    _ => productStocks
                        .Where(ps => ps.TotalStock >= filters.CantidadMin.Value)
                        .Select(ps => ps.ProductoId)
                        .ToList()
                };

                return query.Where(p => productIds.Contains(p.Id));
            }
            else if (filters.CantidadMax.HasValue)
            {
                var productIds = productStocks
                    .Where(ps => ps.TotalStock <= filters.CantidadMax.Value)
                    .Select(ps => ps.ProductoId)
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
