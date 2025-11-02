using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
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
    public async Task<ActionResult<ApiResponse<IEnumerable<BodegaDto>>>> GetAll([FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var bodegas = await _bodegaService.GetAllAsync(activo, ct);
        return this.OkResponse(bodegas, "Bodegas obtenidas correctamente.");
    }

    /// <summary>
    /// Get a specific bodega by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BodegaDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var bodega = await _bodegaService.GetByIdAsync(id, ct);
        
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        return this.OkResponse(bodega, "Bodega obtenida correctamente.");
    }

    /// <summary>
    /// Create a new bodega
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BodegaDto>>> Create([FromBody] CreateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = await _bodegaService.CreateAsync(dto, ct);
        return this.CreatedResponse(nameof(GetById), new { id = bodega.Id }, bodega, "Bodega creada exitosamente.");
    }

    /// <summary>
    /// Update an existing bodega
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateBodegaDto dto, CancellationToken ct = default)
    {
        await _bodegaService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Bodega actualizada exitosamente.");
    }

    /// <summary>
    /// Activate a bodega
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.ActivateAsync(id, ct);
        return this.NoContentResponse("Bodega activada exitosamente.");
    }

    /// <summary>
    /// Deactivate a bodega (only if no products assigned)
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Bodega desactivada exitosamente.");
    }

    /// <summary>
    /// Permanently delete a bodega (only if no products, invoices, or movements)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _bodegaService.DeletePermanentlyAsync(id, ct);
        return this.NoContentResponse("Bodega eliminada exitosamente.");
    }
}
