using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CategoriaDto> CreateAsync(CreateCategoriaDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingCategoria = await _unitOfWork.Categorias.GetByNameAsync(dto.Nombre, ct);
        if (existingCategoria != null)
        {
            throw new BusinessRuleException($"Ya existe una categoría con el nombre '{dto.Nombre}'.");
        }

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            ImagenCategoriaUrl = dto.ImagenCategoriaUrl?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Categorias.AddAsync(categoria, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoriaDto>(categoria);
    }

    public async Task<CategoriaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        return categoria != null ? _mapper.Map<CategoriaDto>(categoria) : null;
    }

    public async Task<IEnumerable<CategoriaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<Categoria> categorias;

        if (activo.HasValue)
        {
            categorias = await _unitOfWork.Categorias.ListAsync(
                filter: c => c.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            categorias = await _unitOfWork.Categorias.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<PagedResult<CategoriaDto>> GetPagedAsync(CategoriaFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Categorias.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<CategoriaDto>>(items);

        return new PagedResult<CategoriaDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateCategoriaDto dto, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        // Validate name uniqueness (excluding current categoria)
        var existingCategoria = await _unitOfWork.Categorias.GetByNameAsync(dto.Nombre, ct);
        if (existingCategoria != null && existingCategoria.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otra categoría con el nombre '{dto.Nombre}'.");
        }

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Descripcion = dto.Descripcion?.Trim();
        categoria.ImagenCategoriaUrl = dto.ImagenCategoriaUrl?.Trim();

        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        if (categoria.Activo)
        {
            throw new BusinessRuleException("La categoría ya está activa.");
        }

        categoria.Activo = true;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        if (!categoria.Activo)
        {
            throw new BusinessRuleException("La categoría ya está desactivada.");
        }

        // Check if categoria has products
        var hasProducts = await _unitOfWork.Categorias.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede desactivar la categoría porque tiene productos asignados. " +
                "Reasigne los productos a otra categoría antes de desactivarla.");
        }

        categoria.Activo = false;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        // Check if categoria has products
        var hasProducts = await _unitOfWork.Categorias.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la categoría porque tiene productos asignados. " +
                "Elimine o reasigne todos los productos antes de eliminar la categoría.");
        }

        _unitOfWork.Categorias.Remove(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProductInCategoriaDto>> GetProductsInCategoriaAsync(
        Guid categoriaId,
        ProductFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate categoria exists
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", categoriaId);
        }

        // Get all products in this category
        var allProducts = await _unitOfWork.Products.ListAsync(
            filter: p => p.CategoriaId == categoriaId,
            ct: ct
        );

        // If no products in this category, return empty result
        if (!allProducts.Any())
        {
            return new PagedResult<ProductInCategoriaDto>
            {
                Items = new List<ProductInCategoriaDto>(),
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = 0
            };
        }

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

        // Map to DTOs
        var productDtos = new List<ProductInCategoriaDto>();

        foreach (var producto in paginatedProducts)
        {
            // Map base properties
            var productDto = _mapper.Map<ProductInCategoriaDto>(producto);

            // Calculate total stock across all warehouses for this product
            var totalStock = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            productDto.StockActual = totalStock;

            // Set quantity in category (same as total stock since product belongs to one category)
            productDto.CantidadEnCategoria = totalStock;

            // Load category name (already known from the query, but populate for consistency)
            productDto.CategoriaNombre = categoria.Nombre;

            productDtos.Add(productDto);
        }

        return new PagedResult<ProductInCategoriaDto>
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
