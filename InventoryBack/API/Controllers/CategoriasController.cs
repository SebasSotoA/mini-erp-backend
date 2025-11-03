using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService ?? throw new ArgumentNullException(nameof(categoriaService));
    }

    /// <summary>
    /// Get all categorias with filtering, sorting, and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoriaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CategoriaDto>>>> GetAll(
        [FromQuery] CategoriaFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _categoriaService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Categorías obtenidas correctamente.");
    }

    /// <summary>
    /// Get a specific categoria by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var categoria = await _categoriaService.GetByIdAsync(id, ct);
        
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        return this.OkResponse(categoria, "Categoría obtenida correctamente.");
    }

    /// <summary>
    /// Create a new categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> Create([FromBody] CreateCategoriaDto dto, CancellationToken ct = default)
    {
        var categoria = await _categoriaService.CreateAsync(dto, ct);
        return this.CreatedResponse(nameof(GetById), new { id = categoria.Id }, categoria, "Categoría creada exitosamente.");
    }

    /// <summary>
    /// Update an existing categoria
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCategoriaDto dto, CancellationToken ct = default)
    {
        await _categoriaService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Categoría actualizada exitosamente.");
    }

    /// <summary>
    /// Activate a categoria
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.ActivateAsync(id, ct);
        return this.NoContentResponse("Categoría activada exitosamente.");
    }

    /// <summary>
    /// Deactivate a categoria (only if no products assigned)
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Categoría desactivada exitosamente.");
    }

    /// <summary>
    /// Permanently delete a categoria (only if no products)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.DeletePermanentlyAsync(id, ct);
        return this.NoContentResponse("Categoría eliminada exitosamente.");
    }
}
