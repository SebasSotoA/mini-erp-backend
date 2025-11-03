using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
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
    /// Get all campos extra with filtering, sorting, and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CampoExtraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CampoExtraDto>>>> GetAll(
        [FromQuery] CampoExtraFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _campoExtraService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Campos extra obtenidos correctamente.");
    }

    /// <summary>
    /// Get a specific campo extra by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampoExtraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CampoExtraDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var campo = await _campoExtraService.GetByIdAsync(id, ct);
        
        if (campo == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        return this.OkResponse(campo, "Campo extra obtenido correctamente.");
    }

    /// <summary>
    /// Create a new campo extra
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampoExtraDto>>> Create([FromBody] CreateCampoExtraDto dto, CancellationToken ct = default)
    {
        var campo = await _campoExtraService.CreateAsync(dto, ct);
        return this.CreatedResponse(nameof(GetById), new { id = campo.Id }, campo, "Campo extra creado exitosamente.");
    }

    /// <summary>
    /// Update an existing campo extra
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCampoExtraDto dto, CancellationToken ct = default)
    {
        await _campoExtraService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Campo extra actualizado exitosamente.");
    }

    /// <summary>
    /// Activate a campo extra
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.ActivateAsync(id, ct);
        return this.NoContentResponse("Campo extra activado exitosamente.");
    }

    /// <summary>
    /// Deactivate a campo extra
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Campo extra desactivado exitosamente.");
    }

    /// <summary>
    /// Permanently delete a campo extra (only if not being used in products)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePermanently(Guid id, CancellationToken ct = default)
    {
        await _campoExtraService.DeletePermanentlyAsync(id, ct);
        return this.NoContentResponse("Campo extra eliminado exitosamente.");
    }
}
