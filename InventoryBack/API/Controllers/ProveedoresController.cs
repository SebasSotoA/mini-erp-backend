using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/proveedores")]
[Produces("application/json")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService ?? throw new ArgumentNullException(nameof(proveedorService));
    }

    /// <summary>
    /// Gets all suppliers.
    /// </summary>
    /// <param name="soloActivos">Filter by active status (null = all, true = active only, false = inactive only)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProveedorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProveedorDto>>>> GetAll(
        [FromQuery] bool? soloActivos = null,
        CancellationToken ct = default)
    {
        var proveedores = await _proveedorService.GetAllAsync(soloActivos, ct);
        return this.OkResponse(proveedores, "Proveedores obtenidos correctamente.");
    }

    /// <summary>
    /// Gets a supplier by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProveedorDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id, ct);
        if (proveedor == null)
        {
            throw new NotFoundException("Proveedor", id);
        }

        return this.OkResponse(proveedor, "Proveedor obtenido correctamente.");
    }

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProveedorDto>>> Create(
        [FromBody] CreateProveedorDto dto,
        CancellationToken ct = default)
    {
        var proveedor = await _proveedorService.CreateAsync(dto, ct);
        return this.CreatedResponse(
            nameof(GetById),
            new { id = proveedor.Id },
            proveedor,
            "Proveedor creado exitosamente."
        );
    }

    /// <summary>
    /// Updates a supplier.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] UpdateProveedorDto dto,
        CancellationToken ct = default)
    {
        await _proveedorService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Proveedor actualizado exitosamente.");
    }

    /// <summary>
    /// Activates a supplier.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct = default)
    {
        await _proveedorService.ActivateAsync(id, ct);
        return this.NoContentResponse("Proveedor activado exitosamente.");
    }

    /// <summary>
    /// Deactivates a supplier (soft delete).
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _proveedorService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Proveedor desactivado exitosamente.");
    }
}
