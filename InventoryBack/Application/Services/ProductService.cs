using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Application.Validators;
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
            StockActual = dto.CantidadInicial, // Mapeo: DTO.CantidadInicial ? Entity.StockActual
            CantidadMinima = null,
            CantidadMaxima = null
        };

        await _unitOfWork.ProductoBodegas.AddAsync(mainProductoBodega, ct);

        // ========== 5. VALIDATE REQUIRED EXTRA FIELDS ==========

        // Get all required campos extra
        var requiredCampos = await _unitOfWork.CamposExtras.ListAsync(
            filter: ce => ce.EsRequerido && ce.Activo,
            ct: ct);

        if (requiredCampos.Any())
        {
            var providedCampoIds = dto.CamposExtra?
                .Where(c => c.CampoExtraId != Guid.Empty && !string.IsNullOrWhiteSpace(c.Valor))
                .Select(c => c.CampoExtraId)
                .ToHashSet() ?? new HashSet<Guid>();

            var missingCampos = requiredCampos
                .Where(rc => !providedCampoIds.Contains(rc.Id))
                .ToList();

            if (missingCampos.Any())
            {
                var missingNames = string.Join(", ", missingCampos.Select(c => $"'{c.Nombre}'"));
                throw new BusinessRuleException(
                    $"Los siguientes campos son requeridos y deben tener un valor: {missingNames}.");
            }
        }

        // ========== 6. CREATE ADDITIONAL WAREHOUSE RELATIONSHIPS (IF PROVIDED) ==========

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
                        StockActual = bodegaDto.CantidadInicial, // Mapeo: DTO.CantidadInicial ? Entity.StockActual
                        CantidadMinima = bodegaDto.CantidadMinima,
                        CantidadMaxima = bodegaDto.CantidadMaxima
                    };

                    await _unitOfWork.ProductoBodegas.AddAsync(productoBodega, ct);
                }
            }
        }

        // ========== 7. LINK EXTRA FIELDS (IF PROVIDED) ==========

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

        // ========== 8. SAVE ALL CHANGES ==========

        await _unitOfWork.SaveChangesAsync(ct);

        // ========== 9. RETURN PRODUCT DTO ==========

        var productDto = _mapper.Map<ProductDto>(producto);
        
        // Calculate total stock across all warehouses
        productDto.StockActual = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);

        // Load category name if product has a category
        if (producto.CategoriaId.HasValue)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(producto.CategoriaId.Value, ct);
            productDto.CategoriaNombre = categoria?.Nombre;
        }

        return productDto;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (producto == null)
            return null;

        var productDto = _mapper.Map<ProductDto>(producto);
        
        // Calculate total stock across all warehouses
        productDto.StockActual = await _unitOfWork.Products.GetTotalStockAsync(id, ct);

        // Load category name if product has a category
        if (producto.CategoriaId.HasValue)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(producto.CategoriaId.Value, ct);
            productDto.CategoriaNombre = categoria?.Nombre;
        }

        return productDto;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        ProductFilterDto filters,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items).ToList();

        // Batch query to get stock for all products at once (more efficient)
        var productIds = dtos.Select(d => d.Id).ToList();
        var stockDictionary = await _unitOfWork.Products.GetTotalStockBatchAsync(productIds, ct);

        // Get all unique category IDs
        var categoryIds = items
            .Where(p => p.CategoriaId.HasValue)
            .Select(p => p.CategoriaId!.Value)
            .Distinct()
            .ToList();

        // Batch query to get all categories at once
        Dictionary<Guid, string> categoryNames = new();
        if (categoryIds.Any())
        {
            var categories = await _unitOfWork.Categorias.ListAsync(
                filter: c => categoryIds.Contains(c.Id),
                ct: ct
            );
            categoryNames = categories.ToDictionary(c => c.Id, c => c.Nombre);
        }

        // Populate StockActual and CategoriaNombre for each product
        foreach (var dto in dtos)
        {
            dto.StockActual = stockDictionary.GetValueOrDefault(dto.Id, 0);
            
            if (dto.CategoriaId.HasValue && categoryNames.ContainsKey(dto.CategoriaId.Value))
            {
                dto.CategoriaNombre = categoryNames[dto.CategoriaId.Value];
            }
        }

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

    // ========== WAREHOUSE MANAGEMENT ==========

    public async Task AddToWarehouseAsync(Guid productId, AddProductoBodegaDto dto, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        // Validate warehouse exists and is active
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(dto.BodegaId, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", dto.BodegaId);
        }

        if (!bodega.Activo)
        {
            throw new BusinessRuleException($"La bodega '{bodega.Nombre}' está inactiva.");
        }

        // Check if product is already in this warehouse
        var existing = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(productId, dto.BodegaId, ct);
        if (existing != null)
        {
            throw new BusinessRuleException($"El producto ya está asignado a la bodega '{bodega.Nombre}'.");
        }

        // Create relationship
        var productoBodega = new ProductoBodega
        {
            Id = Guid.NewGuid(),
            ProductoId = productId,
            BodegaId = dto.BodegaId,
            StockActual = dto.CantidadInicial, // Mapeo: DTO.CantidadInicial ? Entity.StockActual
            CantidadMinima = dto.CantidadMinima,
            CantidadMaxima = dto.CantidadMaxima
        };

        await _unitOfWork.ProductoBodegas.AddAsync(productoBodega, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateWarehouseQuantitiesAsync(Guid productId, Guid bodegaId, UpdateProductoBodegaDto dto, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        // Get product-warehouse relationship
        var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(productId, bodegaId, ct);
        if (productoBodega == null)
        {
            throw new NotFoundException($"Producto-Bodega", $"{productId}/{bodegaId}");
        }

        // Update quantities
        productoBodega.StockActual = dto.CantidadInicial; // Mapeo: DTO.CantidadInicial ? Entity.StockActual
        productoBodega.CantidadMinima = dto.CantidadMinima;
        productoBodega.CantidadMaxima = dto.CantidadMaxima;

        _unitOfWork.ProductoBodegas.Update(productoBodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveFromWarehouseAsync(Guid productId, Guid bodegaId, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        // Get product-warehouse relationship
        var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(productId, bodegaId, ct);
        if (productoBodega == null)
        {
            throw new NotFoundException($"Producto-Bodega", $"{productId}/{bodegaId}");
        }

        // Check if this is the last warehouse
        var allWarehouses = await _unitOfWork.ProductoBodegas.GetByProductIdAsync(productId, ct);
        if (allWarehouses.Count() <= 1)
        {
            throw new BusinessRuleException("No se puede eliminar la última bodega del producto. Un producto debe estar al menos en una bodega.");
        }

        _unitOfWork.ProductoBodegas.Remove(productoBodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ProductoBodegaDetailDto>> GetProductWarehousesAsync(Guid productId, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        var productoBodegas = await _unitOfWork.Products.GetProductWarehousesAsync(productId, ct);

        var result = new List<ProductoBodegaDetailDto>();

        foreach (var pb in productoBodegas)
        {
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(pb.BodegaId, ct);
            if (bodega != null)
            {
                result.Add(new ProductoBodegaDetailDto
                {
                    BodegaId = pb.BodegaId,
                    BodegaNombre = bodega.Nombre,
                    BodegaDireccion = bodega.Direccion,
                    CantidadInicial = pb.StockActual, // Mapeo inverso: Entity.StockActual ? DTO.CantidadInicial
                    CantidadMinima = pb.CantidadMinima,
                    CantidadMaxima = pb.CantidadMaxima
                });
            }
        }

        return result;
    }

    // ========== EXTRA FIELDS MANAGEMENT ==========

    public async Task SetExtraFieldAsync(Guid productId, Guid campoExtraId, string valor, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        // Validate campo extra exists and is active
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(campoExtraId, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", campoExtraId);
        }

        if (!campoExtra.Activo)
        {
            throw new BusinessRuleException($"El campo extra '{campoExtra.Nombre}' está inactivo.");
        }

        // Validate value matches tipo dato
        if (!CampoExtraValueValidator.IsValidValorPorDefecto(campoExtra.TipoDato, valor, out var errorMessage))
        {
            throw new BusinessRuleException(errorMessage ?? "El valor no es compatible con el tipo de dato.");
        }

        // Check if relationship already exists
        var existingList = await _unitOfWork.ProductoCamposExtras.ListAsync(
            filter: pce => pce.ProductoId == productId && pce.CampoExtraId == campoExtraId,
            ct: ct);
        var existing = existingList.FirstOrDefault();

        if (existing != null)
        {
            // Update existing
            existing.Valor = valor.Trim();
            _unitOfWork.ProductoCamposExtras.Update(existing);
        }
        else
        {
            // Create new
            var productoCampoExtra = new ProductoCampoExtra
            {
                Id = Guid.NewGuid(),
                ProductoId = productId,
                CampoExtraId = campoExtraId,
                Valor = valor.Trim()
            };
            await _unitOfWork.ProductoCamposExtras.AddAsync(productoCampoExtra, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveExtraFieldAsync(Guid productId, Guid campoExtraId, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        // Get relationship
        var productoCampoExtraList = await _unitOfWork.ProductoCamposExtras.ListAsync(
            filter: pce => pce.ProductoId == productId && pce.CampoExtraId == campoExtraId,
            ct: ct);
        var productoCampoExtra = productoCampoExtraList.FirstOrDefault();

        if (productoCampoExtra == null)
        {
            throw new NotFoundException($"Producto-CampoExtra", $"{productId}/{campoExtraId}");
        }

        _unitOfWork.ProductoCamposExtras.Remove(productoCampoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ProductoCampoExtraDetailDto>> GetProductExtraFieldsAsync(Guid productId, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", productId);
        }

        var productoCamposExtra = await _unitOfWork.Products.GetProductExtraFieldsAsync(productId, ct);

        var result = new List<ProductoCampoExtraDetailDto>();

        foreach (var pce in productoCamposExtra)
        {
            var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(pce.CampoExtraId, ct);
            if (campoExtra != null)
            {
                result.Add(new ProductoCampoExtraDetailDto
                {
                    CampoExtraId = pce.CampoExtraId,
                    Nombre = campoExtra.Nombre,
                    TipoDato = campoExtra.TipoDato,
                    Valor = pce.Valor,
                    EsRequerido = campoExtra.EsRequerido,
                    Descripcion = campoExtra.Descripcion
                });
            }
        }

        return result;
    }
}
