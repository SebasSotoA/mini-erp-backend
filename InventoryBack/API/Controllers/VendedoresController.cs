using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/vendedores")]
[Produces("application/json")]
public class VendedoresController : ControllerBase
{
    private readonly IVendedorService _vendedorService;

    public VendedoresController(IVendedorService vendedorService)
    {
        _vendedorService = vendedorService ?? throw new ArgumentNullException(nameof(vendedorService));
    }

    /// <summary>
    /// Gets all salespersons with filtering, sorting, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendedorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<VendedorDto>>>> GetAll(
        [FromQuery] VendedorFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _vendedorService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Vendedores obtenidos correctamente.");
    }

    /// <summary>
    /// Gets a salesperson by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VendedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendedorDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var vendedor = await _vendedorService.GetByIdAsync(id, ct);
        if (vendedor == null)
        {
            throw new NotFoundException("Vendedor", id);
        }

        return this.OkResponse(vendedor, "Vendedor obtenido correctamente.");
    }

    /// <summary>
    /// Creates a new salesperson.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<VendedorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VendedorDto>>> Create(
        [FromBody] CreateVendedorDto dto,
        CancellationToken ct = default)
    {
        var vendedor = await _vendedorService.CreateAsync(dto, ct);
        return this.CreatedResponse(
            nameof(GetById),
            new { id = vendedor.Id },
            vendedor,
            "Vendedor creado exitosamente."
        );
    }

    /// <summary>
    /// Updates a salesperson.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] UpdateVendedorDto dto,
        CancellationToken ct = default)
    {
        await _vendedorService.UpdateAsync(id, dto, ct);
        return this.NoContentResponse("Vendedor actualizado exitosamente.");
    }

    /// <summary>
    /// Activates a salesperson.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct = default)
    {
        await _vendedorService.ActivateAsync(id, ct);
        return this.NoContentResponse("Vendedor activado exitosamente.");
    }

    /// <summary>
    /// Deactivates a salesperson (soft delete).
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct = default)
    {
        await _vendedorService.DeactivateAsync(id, ct);
        return this.NoContentResponse("Vendedor desactivado exitosamente.");
    }
}
