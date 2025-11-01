using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/campos-extra")]
public class CamposExtraController : ControllerBase
{
    private readonly ICampoExtraService _campoExtraService;

    public CamposExtraController(ICampoExtraService campoExtraService)
    {
        _campoExtraService = campoExtraService ?? throw new ArgumentNullException(nameof(campoExtraService));
    }

    /// <summary>
    /// Get all campos extra, optionally filtered by active status
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampoExtraDto>>> GetAll([FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var campos = await _campoExtraService.GetAllAsync(activo, ct);
        return Ok(campos);
    }

    /// <summary>
    /// Get a specific campo extra by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CampoExtraDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var campo = await _campoExtraService.GetByIdAsync(id, ct);
        
        if (campo == null)
        {
            return NotFound(new { message = $"Campo extra con ID {id} no encontrado." });
        }

        return Ok(campo);
    }

    /// <summary>
    /// Create a new campo extra
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CampoExtraDto>> Create([FromBody] CreateCampoExtraDto dto, CancellationToken ct = default)
    {
        var campo = await _campoExtraService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = campo.Id }, campo);
    }

    /// <summary>
    /// Update an existing campo extra
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampoExtraDto dto, CancellationToken ct = default)
    {
        await _campoExtraService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Activate a campo extra
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.ActivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a campo extra
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.DeactivateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Permanently delete a campo extra
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.DeletePermanentlyAsync(id, ct);
        return NoContent();
    }
}
