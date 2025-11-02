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
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
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
}
