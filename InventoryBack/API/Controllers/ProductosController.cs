using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Services;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[ApiController]
[Route("api/productos")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductosController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// Gets a paginated list of products with advanced filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProducts(
        [FromQuery] ProductFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _productService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Productos obtenidos correctamente.");
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            throw new NotFoundException("Producto", id);
        }

        return this.OkResponse(product, "Producto obtenido correctamente.");
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken ct = default)
    {
        var product = await _productService.CreateAsync(dto, ct);
        return this.CreatedResponse(
            nameof(GetProduct),
            new { id = product.Id },
            product,
            "Producto creado exitosamente."
        );
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductDto dto,
        CancellationToken ct = default)
    {
        await _productService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Producto actualizado exitosamente.");
    }

    /// <summary>
    /// Activates a product.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ActivateProduct(Guid id, CancellationToken ct = default)
    {
        await _productService.ActivateAsync(id, ct);
        return this.NoContentResponse("Producto activado exitosamente.");
    }

    /// <summary>
    /// Deactivates a product.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateProduct(Guid id, CancellationToken ct = default)
    {
        await _productService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Producto desactivado exitosamente.");
    }

    /// <summary>
    /// Permanently deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}/permanent")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductPermanently(Guid id, CancellationToken ct = default)
    {
        await _productService.DeletePermanentlyAsync(id, ct);
        return this.NoContentResponse("Producto eliminado exitosamente.");
    }

    // ========== WAREHOUSE MANAGEMENT ==========

    /// <summary>
    /// Gets all warehouses where a product is stored.
    /// </summary>
    [HttpGet("{productId:guid}/bodegas")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoBodegaDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductoBodegaDetailDto>>>> GetProductWarehouses(
        Guid productId,
        CancellationToken ct = default)
    {
        var bodegas = await _productService.GetProductWarehousesAsync(productId, ct);
        return this.OkResponse(bodegas, "Bodegas del producto obtenidas correctamente.");
    }

    /// <summary>
    /// Adds a product to an additional warehouse.
    /// </summary>
    [HttpPost("{productId:guid}/bodegas")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> AddProductToWarehouse(
        Guid productId,
        [FromBody] AddProductoBodegaDto dto,
        CancellationToken ct = default)
    {
        await _productService.AddToWarehouseAsync(productId, dto, ct);
        return this.NoContentResponse("Producto agregado a la bodega exitosamente.");
    }

    /// <summary>
    /// Updates product quantities/thresholds in a specific warehouse.
    /// </summary>
    [HttpPut("{productId:guid}/bodegas/{bodegaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProductWarehouseQuantities(
        Guid productId,
        Guid bodegaId,
        [FromBody] UpdateProductoBodegaDto dto,
        CancellationToken ct = default)
    {
        await _productService.UpdateWarehouseQuantitiesAsync(productId, bodegaId, dto, ct);
        return this.NoContentResponse("Cantidades actualizadas exitosamente.");
    }

    /// <summary>
    /// Removes a product from a warehouse.
    /// </summary>
    [HttpDelete("{productId:guid}/bodegas/{bodegaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveProductFromWarehouse(
        Guid productId,
        Guid bodegaId,
        CancellationToken ct = default)
    {
        await _productService.RemoveFromWarehouseAsync(productId, bodegaId, ct);
        return this.NoContentResponse("Producto removido de la bodega exitosamente.");
    }

    // ========== EXTRA FIELDS MANAGEMENT ==========

    /// <summary>
    /// Gets all extra fields assigned to a product.
    /// </summary>
    [HttpGet("{productId:guid}/campos-extra")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoCampoExtraDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductoCampoExtraDetailDto>>>> GetProductExtraFields(
        Guid productId,
        CancellationToken ct = default)
    {
        var campos = await _productService.GetProductExtraFieldsAsync(productId, ct);
        return this.OkResponse(campos, "Campos extra del producto obtenidos correctamente.");
    }

    /// <summary>
    /// Sets or updates an extra field value for a product.
    /// </summary>
    [HttpPut("{productId:guid}/campos-extra/{campoExtraId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> SetProductExtraField(
        Guid productId,
        Guid campoExtraId,
        [FromBody] SetProductoCampoExtraDto dto,
        CancellationToken ct = default)
    {
        await _productService.SetExtraFieldAsync(productId, campoExtraId, dto.Valor, ct);
        return this.NoContentResponse("Campo extra actualizado exitosamente.");
    }

    /// <summary>
    /// Removes an extra field from a product.
    /// </summary>
    [HttpDelete("{productId:guid}/campos-extra/{campoExtraId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveProductExtraField(
        Guid productId,
        Guid campoExtraId,
        CancellationToken ct = default)
    {
        await _productService.RemoveExtraFieldAsync(productId, campoExtraId, ct);
        return this.NoContentResponse("Campo extra removido exitosamente.");
    }
}
