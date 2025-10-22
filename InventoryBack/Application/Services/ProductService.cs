using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

/// <summary>
/// Product service implementation with business logic.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        // Validate SKU uniqueness
        if (!string.IsNullOrWhiteSpace(dto.CodigoSku))
        {
            var skuExists = await _unitOfWork.Products.SkuExistsAsync(dto.CodigoSku, null, ct);
            if (skuExists)
            {
                throw new BusinessRuleException($"Ya existe un producto con el SKU '{dto.CodigoSku}'.");
            }
        }

        // Validate category exists if provided
        if (dto.CategoriaId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoriaId.Value, ct);
            if (category == null)
            {
                throw new NotFoundException("Categoria", dto.CategoriaId.Value);
            }
        }

        // Map and set creation date
        var producto = _mapper.Map<Producto>(dto);
        producto.Id = Guid.NewGuid();
        producto.FechaCreacion = DateTime.UtcNow;
        producto.Activo = true;

        // Save
        await _unitOfWork.Products.AddAsync(producto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<ProductDto>(producto);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        return producto != null ? _mapper.Map<ProductDto>(producto) : null;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchQuery = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            page,
            pageSize,
            searchQuery,
            includeInactive: false,
            ct);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        // Validate SKU uniqueness (excluding current product)
        if (!string.IsNullOrWhiteSpace(dto.CodigoSku))
        {
            var skuExists = await _unitOfWork.Products.SkuExistsAsync(dto.CodigoSku, id, ct);
            if (skuExists)
            {
                throw new BusinessRuleException($"Ya existe otro producto con el SKU '{dto.CodigoSku}'.");
            }
        }

        // Validate category exists if provided
        if (dto.CategoriaId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoriaId.Value, ct);
            if (category == null)
            {
                throw new NotFoundException("Categoria", dto.CategoriaId.Value);
            }
        }

        // Map changes (preserving Id and FechaCreacion)
        _mapper.Map(dto, producto);

        _unitOfWork.Products.Update(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        if (producto.Activo)
        {
            throw new BusinessRuleException("El producto ya está activo.");
        }

        producto.Activo = true;
        _unitOfWork.Products.Update(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        if (!producto.Activo)
        {
            throw new BusinessRuleException("El producto ya está desactivado.");
        }

        producto.Activo = false;
        _unitOfWork.Products.Update(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        // Check if product is referenced in invoices
        var isReferenced = await _unitOfWork.Products.IsProductReferencedAsync(id, ct);
        if (isReferenced)
        {
            throw new BusinessRuleException(
                "No se puede eliminar permanentemente el producto porque está referenciado en facturas de venta o compra. " +
                "Considere desactivarlo en su lugar.");
        }

        // Permanent delete - physically remove from database
        _unitOfWork.Products.Remove(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
