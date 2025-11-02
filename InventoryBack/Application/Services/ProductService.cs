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

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISkuGeneratorService skuGenerator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _skuGenerator = skuGenerator ?? throw new ArgumentNullException(nameof(skuGenerator));
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
        var mainWarehouse = await _unitOfWork.Bodegas.GetByIdAsync(dto.BodegaPrincipalId, ct);
        if (mainWarehouse == null)
        {
            throw new NotFoundException("Bodega", dto.BodegaPrincipalId);
        }

        if (!mainWarehouse.Activo)
        {
            throw new BusinessRuleException($"La bodega '{mainWarehouse.Nombre}' está inactiva.");
        }

        // Validate: Category exists if provided
        if (dto.CategoriaId.HasValue)
        {
            var category = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId.Value, ct);
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

        // ========== 3. CREATE PRODUCT ENTITY ==========

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
            ImagenProductoUrl = dto.ImagenProductoUrl?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(producto, ct);

        // ========== 4. CREATE MAIN WAREHOUSE RELATIONSHIP ==========

        var mainProductoBodega = new ProductoBodega
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            BodegaId = dto.BodegaPrincipalId,
            CantidadInicial = dto.CantidadInicial,
            CantidadMinima = null,
            CantidadMaxima = null
        };

        await _unitOfWork.ProductoBodegas.AddAsync(mainProductoBodega, ct);

        // ========== 5. CREATE ADDITIONAL WAREHOUSE RELATIONSHIPS (IF PROVIDED) ==========

        if (dto.BodegasAdicionales != null && dto.BodegasAdicionales.Any())
        {
            // Filter out entries with empty GUIDs or invalid data
            var validBodegas = dto.BodegasAdicionales
                .Where(b => b.BodegaId != Guid.Empty)
                .ToList();

            if (validBodegas.Any())
            {
                // Collect all warehouse IDs (including main)
                var allBodegaIds = validBodegas.Select(b => b.BodegaId).ToList();
                allBodegaIds.Add(dto.BodegaPrincipalId);

                // Validate no duplicate warehouses
                if (allBodegaIds.Count != allBodegaIds.Distinct().Count())
                {
                    throw new BusinessRuleException(
                        "No se pueden agregar bodegas duplicadas. " +
                        "Verifique que no se repita la bodega principal ni las bodegas adicionales.");
                }

                foreach (var bodegaDto in validBodegas)
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
                        CantidadInicial = bodegaDto.CantidadInicial,
                        CantidadMinima = bodegaDto.CantidadMinima,
                        CantidadMaxima = bodegaDto.CantidadMaxima
                    };

                    await _unitOfWork.ProductoBodegas.AddAsync(productoBodega, ct);
                }
            }
        }

        // ========== 6. LINK EXTRA FIELDS (IF PROVIDED) ==========

        if (dto.CamposExtra != null && dto.CamposExtra.Any())
        {
            // Filter out entries with empty GUIDs or empty values
            var validCampos = dto.CamposExtra
                .Where(c => c.CampoExtraId != Guid.Empty)
                .ToList();

            if (validCampos.Any())
            {
                // Get all campos extra to validate
                var campoExtraIds = validCampos.Select(c => c.CampoExtraId).Distinct().ToList();
                
                foreach (var campoDto in validCampos)
                {
                    // Skip if valor is empty and campo is not required (we'll validate this below)
                    if (string.IsNullOrWhiteSpace(campoDto.Valor))
                    {
                        // Check if this campo is required
                        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(campoDto.CampoExtraId, ct);
                        if (campoExtra == null)
                        {
                            throw new NotFoundException("CampoExtra", campoDto.CampoExtraId);
                        }

                        if (!campoExtra.Activo)
                        {
                            throw new BusinessRuleException($"El campo extra '{campoExtra.Nombre}' está inactivo.");
                        }

                        // If campo is required, throw error
                        if (campoExtra.EsRequerido)
                        {
                            throw new BusinessRuleException($"El campo '{campoExtra.Nombre}' es requerido y debe tener un valor.");
                        }

                        // Skip optional campo with empty value
                        continue;
                    }

                    // Validate that CampoExtra exists
                    var campo = await _unitOfWork.CamposExtras.GetByIdAsync(campoDto.CampoExtraId, ct);
                    if (campo == null)
                    {
                        throw new NotFoundException("CampoExtra", campoDto.CampoExtraId);
                    }

                    if (!campo.Activo)
                    {
                        throw new BusinessRuleException($"El campo extra '{campo.Nombre}' está inactivo.");
                    }

                    // Create the ProductoCampoExtra relationship
                    var productoCampoExtra = new ProductoCampoExtra
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        CampoExtraId = campo.Id,
                        Valor = campoDto.Valor.Trim()
                    };

                    await _unitOfWork.ProductoCamposExtras.AddAsync(productoCampoExtra, ct);
                }
            }
        }

        // ========== 7. SAVE ALL CHANGES ==========

        await _unitOfWork.SaveChangesAsync(ct);

        // ========== 8. RETURN PRODUCT DTO ==========

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
            var category = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId.Value, ct);
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

        // ========== CHECK ALL DEPENDENCIES ==========

        var dependencies = await _unitOfWork.Products.GetProductDependenciesAsync(id, ct);

        var errors = new List<string>();

        // Check invoices (critical - cannot delete)
        if (dependencies["facturasVenta"] > 0)
        {
            errors.Add($"Está referenciado en {dependencies["facturasVenta"]} factura(s) de venta");
        }

        if (dependencies["facturasCompra"] > 0)
        {
            errors.Add($"Está referenciado en {dependencies["facturasCompra"]} factura(s) de compra");
        }

        // Check inventory movements (critical - cannot delete)
        if (dependencies["movimientosInventario"] > 0)
        {
            errors.Add($"Tiene {dependencies["movimientosInventario"]} movimiento(s) de inventario registrados");
        }

        if (errors.Any())
        {
            var errorMessage = "No se puede eliminar permanentemente el producto porque:\n- " +
                               string.Join("\n- ", errors) +
                               "\n\nConsidere desactivarlo en su lugar.";
            throw new BusinessRuleException(errorMessage);
        }

        // ========== WARNINGS (will be cascade deleted) ==========

        var warnings = new List<string>();

        if (dependencies["bodegas"] > 0)
        {
            warnings.Add($"{dependencies["bodegas"]} relación(es) con bodegas");
        }

        if (dependencies["camposExtra"] > 0)
        {
            warnings.Add($"{dependencies["camposExtra"]} campo(s) extra asignados");
        }

        // Note: These will be cascade deleted due to OnDelete(DeleteBehavior.Cascade) in DbContext
        // ProductoBodegas and ProductoCamposExtra have cascade delete configured

        // Permanent delete - physically remove from database
        // This will cascade delete ProductoBodegas and ProductoCamposExtra
        _unitOfWork.Products.Remove(producto);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
