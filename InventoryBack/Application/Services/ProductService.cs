using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

/// <summary>
/// Product service implementation with business logic for both quick and advanced creation flows.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISkuGeneratorService _skuGenerator;
    private readonly IStorageService _storageService;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISkuGeneratorService skuGenerator,
        IStorageService storageService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _skuGenerator = skuGenerator ?? throw new ArgumentNullException(nameof(skuGenerator));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        // ========== 1. VALIDATE BUSINESS RULES ==========

        // Validate: PrecioBase > CostoInicial
        if (dto.PrecioBase <= dto.CostoInicial)
        {
            throw new BusinessRuleException("El precio base debe ser mayor que el costo inicial.");
        }

        // Validate: Main warehouse exists
        var mainWarehouse = await _unitOfWork.Bodegas.GetByIdAsync(dto.BodegaId, ct);
        if (mainWarehouse == null)
        {
            throw new NotFoundException("Bodega", dto.BodegaId);
        }

        if (!mainWarehouse.Activo)
        {
            throw new BusinessRuleException($"La bodega '{mainWarehouse.Nombre}' está inactiva.");
        }

        // Validate: Category exists if provided
        if (dto.CategoriaId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoriaId.Value, ct);
            if (category == null)
            {
                throw new NotFoundException("Categoria", dto.CategoriaId.Value);
            }

            if (!category.Activo)
            {
                throw new BusinessRuleException($"La categoría '{category.Nombre}' está inactiva.");
            }
        }

        // ========== 2. GENERATE OR VALIDATE SKU ==========

        string sku;
        if (string.IsNullOrWhiteSpace(dto.CodigoSku))
        {
            // Auto-generate SKU
            sku = await _skuGenerator.GenerateSkuAsync(dto.Nombre, dto.CategoriaId, ct);
        }
        else
        {
            // Validate SKU uniqueness
            sku = dto.CodigoSku.Trim();
            var skuExists = await _unitOfWork.Products.SkuExistsAsync(sku, null, ct);
            if (skuExists)
            {
                throw new BusinessRuleException($"Ya existe un producto con el SKU '{sku}'.");
            }
        }

        // ========== 3. UPLOAD IMAGE TO SUPABASE (IF PROVIDED) ==========

        string? imageUrl = null;
        if (dto.Imagen != null)
        {
            try
            {
                imageUrl = await _storageService.UploadImageAsync(dto.Imagen, "products", ct);
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException($"Error al subir la imagen: {ex.Message}");
            }
        }

        // ========== 4. CREATE PRODUCT ENTITY ==========

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            UnidadMedida = dto.UnidadMedida.Trim(),
            PrecioBase = dto.PrecioBase,
            ImpuestoPorcentaje = dto.ImpuestoPorcentaje,
            CostoInicial = dto.CostoInicial,
            CategoriaId = dto.CategoriaId,
            CodigoSku = sku,
            Descripcion = dto.Descripcion?.Trim(),
            ImagenProductoUrl = imageUrl,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(producto, ct);

        // ========== 5. CREATE MAIN WAREHOUSE RELATIONSHIP ==========

        var mainProductoBodega = new ProductoBodega
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            BodegaId = dto.BodegaId,
            CantidadInicial = dto.Cantidad,
            CantidadMinima = null,
            CantidadMaxima = null
        };

        await _unitOfWork.ProductoBodegas.AddAsync(mainProductoBodega, ct);

        // ========== 6. CREATE ADDITIONAL WAREHOUSE RELATIONSHIPS (IF PROVIDED) ==========

        if (dto.BodegasAdicionales != null && dto.BodegasAdicionales.Any())
        {
            // Validate no duplicate warehouses
            var allBodegaIds = dto.BodegasAdicionales.Select(b => b.BodegaId).ToList();
            allBodegaIds.Add(dto.BodegaId); // Include main warehouse

            if (allBodegaIds.Count != allBodegaIds.Distinct().Count())
            {
                throw new BusinessRuleException("No se pueden agregar bodegas duplicadas (incluyendo la bodega principal).");
            }

            foreach (var bodegaDto in dto.BodegasAdicionales)
            {
                // Validate warehouse exists and is active
                var bodega = await _unitOfWork.Bodegas.GetByIdAsync(bodegaDto.BodegaId, ct);
                if (bodega == null)
                {
                    throw new NotFoundException("Bodega", bodegaDto.BodegaId);
                }

                if (!bodega.Activo)
                {
                    throw new BusinessRuleException($"La bodega '{bodega.Nombre}' está inactiva.");
                }

                var productoBodega = new ProductoBodega
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    BodegaId = bodegaDto.BodegaId,
                    CantidadInicial = bodegaDto.Cantidad,
                    CantidadMinima = bodegaDto.CantidadMinima,
                    CantidadMaxima = bodegaDto.CantidadMaxima
                };

                await _unitOfWork.ProductoBodegas.AddAsync(productoBodega, ct);
            }
        }

        // ========== 7. CREATE ADDITIONAL FIELDS (IF PROVIDED) ==========

        if (dto.CamposAdicionales != null && dto.CamposAdicionales.Any())
        {
            foreach (var campoDto in dto.CamposAdicionales)
            {
                var productoCampoExtra = new ProductoCampoExtra
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    CampoExtraId = Guid.Empty, // Dynamic field (not linked to predefined CampoExtra)
                    Valor = $"{campoDto.Nombre.Trim()}: {campoDto.Valor.Trim()}"
                };

                await _unitOfWork.ProductoCamposExtras.AddAsync(productoCampoExtra, ct);
            }
        }

        // ========== 8. SAVE ALL CHANGES ==========

        await _unitOfWork.SaveChangesAsync(ct);

        // ========== 9. RETURN PRODUCT DTO ==========

        return _mapper.Map<ProductDto>(producto);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        return producto != null ? _mapper.Map<ProductDto>(producto) : null;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        ProductFilterDto filters,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
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

        // Delete image from storage if exists
        if (!string.IsNullOrEmpty(producto.ImagenProductoUrl))
        {
            try
            {
                await _storageService.DeleteImageAsync(producto.ImagenProductoUrl, "products", ct);
            }
            catch
            {
                // Log warning but continue with deletion
            }
        }

        // Permanent delete - physically remove from database
        _unitOfWork.Products.Remove(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
