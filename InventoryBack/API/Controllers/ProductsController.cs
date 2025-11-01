using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Services;
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
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a paginated list of products with advanced filtering.
    /// </summary>
    /// <param name="filters">Filter criteria (from query string)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductFilterDto filters,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _productService.GetPagedAsync(filters, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { message = "Error al obtener los productos." });
        }
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken ct = default)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, ct);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con ID '{id}' no encontrado." });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { message = "Error al obtener el producto." });
        }
    }

    /// <summary>
    /// Creates a new product. Supports both quick and advanced creation flows.
    /// Quick flow: Nombre, UnidadMedida, bodegaPrincipalId, PrecioBase, CostoInicial, cantidadInicial
    /// Advanced flow: All quick fields + CodigoSku, CategoriaId, Descripcion, imagenProductoUrl, BodegasAdicionales, CamposExtra
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <remarks>
    /// Example quick flow request (JSON):
    /// ```json
    /// {
    ///   "nombre": "Laptop HP",
    ///   "unidadMedida": "Unidad",
    ///   "bodegaPrincipalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "precioBase": 2500000,
    ///   "costoInicial": 2000000,
    ///   "cantidadInicial": 10,
    ///   "impuestoPorcentaje": 19
    /// }
    /// ```
    /// 
    /// Example advanced flow request (JSON):
    /// ```json
    /// {
    ///   "nombre": "Monitor LG UltraWide",
    ///   "unidadMedida": "Unidad",
    ///   "bodegaPrincipalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "precioBase": 1500000,
    ///   "costoInicial": 1200000,
    ///   "cantidadInicial": 5,
    ///   "impuestoPorcentaje": 19,
    ///   "codigoSku": "MON-LG-001",
    ///   "categoriaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "descripcion": "Monitor ultrawide 34 pulgadas",
    ///   "imagenProductoUrl": "https://your-project.supabase.co/storage/v1/object/public/products/monitor.jpg",
    ///   "bodegasAdicionales": [{"bodegaId": "...", "cantidadInicial": 3}],
    ///   "camposExtra": [{"campoExtraId": "...", "valor": "2 años"}]
    /// }
    /// ```
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var product = await _productService.CreateAsync(dto, ct);
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation when creating product");
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Related entity not found when creating product");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Error al crear el producto." });
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductDto dto,
        CancellationToken ct = default)
    {
        try
        {
            await _productService.UpdateAsync(id, dto, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found when updating: {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation when updating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new { message = "Error al actualizar el producto." });
        }
    }

    /// <summary>
    /// Activates a product (sets Activo to true).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateProduct(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _productService.ActivateAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found when activating: {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation when activating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product {ProductId}", id);
            return StatusCode(500, new { message = "Error al activar el producto." });
        }
    }

    /// <summary>
    /// Deactivates a product (sets Activo to false).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateProduct(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _productService.DeactivateAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found when deactivating: {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation when deactivating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", id);
            return StatusCode(500, new { message = "Error al desactivar el producto." });
        }
    }

    /// <summary>
    /// Permanently deletes a product from the database.
    /// Only allowed if the product is not referenced in any invoices.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductPermanently(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _productService.DeletePermanentlyAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found when permanently deleting: {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation when permanently deleting product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting product {ProductId}", id);
            return StatusCode(500, new { message = "Error al eliminar permanentemente el producto." });
        }
    }
}
