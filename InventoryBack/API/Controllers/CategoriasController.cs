using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
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
    /// Get all categorias, optionally filtered by active status
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll([FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var categorias = await _categoriaService.GetAllAsync(activo, ct);
        return Ok(categorias);
    }

    /// <summary>
    /// Get a specific categoria by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var categoria = await _categoriaService.GetByIdAsync(id, ct);
        
        if (categoria == null)
        {
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        }

        return Ok(categoria);
    }

    /// <summary>
    /// Create a new categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CreateCategoriaDto dto, CancellationToken ct = default)
    {
        var categoria = await _categoriaService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    /// <summary>
    /// Update an existing categoria
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoriaDto dto, CancellationToken ct = default)
    {
        await _categoriaService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Activate a categoria
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.ActivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a categoria (only if no products assigned)
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.DeactivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Permanently delete a categoria (only if no products)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _categoriaService.DeletePermanentlyAsync(id, ct);
        return NoContent();
    }
}
