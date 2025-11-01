using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/bodegas")]
public class BodegasController : ControllerBase
{
    private readonly IBodegaService _bodegaService;

    public BodegasController(IBodegaService bodegaService)
    {
        _bodegaService = bodegaService ?? throw new ArgumentNullException(nameof(bodegaService));
    }

    /// <summary>
    /// Get all bodegas, optionally filtered by active status
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BodegaDto>>> GetAll([FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var bodegas = await _bodegaService.GetAllAsync(activo, ct);
        return Ok(bodegas);
    }

    /// <summary>
    /// Get a specific bodega by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BodegaDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var bodega = await _bodegaService.GetByIdAsync(id, ct);
        
        if (bodega == null)
        {
            return NotFound(new { message = $"Bodega con ID {id} no encontrada." });
        }

        return Ok(bodega);
    }

    /// <summary>
    /// Create a new bodega
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BodegaDto>> Create([FromBody] CreateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = await _bodegaService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = bodega.Id }, bodega);
    }

    /// <summary>
    /// Update an existing bodega
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBodegaDto dto, CancellationToken ct = default)
    {
        await _bodegaService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Activate a bodega
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.ActivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a bodega (only if no products assigned)
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.DeactivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Permanently delete a bodega (only if no products, invoices, or movements)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.DeletePermanentlyAsync(id, ct);
        return NoContent();
    }
}
