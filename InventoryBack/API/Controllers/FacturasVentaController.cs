using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/facturas-venta")]
[Produces("application/json")]
public class FacturasVentaController : ControllerBase
{
    private readonly IFacturaVentaService _facturaVentaService;

    public FacturasVentaController(IFacturaVentaService facturaVentaService)
    {
        _facturaVentaService = facturaVentaService ?? throw new ArgumentNullException(nameof(facturaVentaService));
    }

    /// <summary>
    /// Gets all sales invoices with filtering, sorting, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FacturaVentaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FacturaVentaDto>>>> GetAll(
        [FromQuery] FacturaVentaFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _facturaVentaService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Facturas de venta obtenidas correctamente.");
    }

    /// <summary>
    /// Gets a sales invoice by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FacturaVentaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FacturaVentaDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var factura = await _facturaVentaService.GetByIdAsync(id, ct);
        if (factura == null)
        {
            throw new NotFoundException("FacturaVenta", id);
        }

        return this.OkResponse(factura, "Factura de venta obtenida correctamente.");
    }

    /// <summary>
    /// Creates a new sales invoice.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<FacturaVentaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FacturaVentaDto>>> Create(
        [FromBody] CreateFacturaVentaDto dto,
        CancellationToken ct = default)
    {
        var factura = await _facturaVentaService.CreateAsync(dto, ct);
        return this.CreatedResponse(
            nameof(GetById),
            new { id = factura.Id },
            factura,
            "Factura de venta creada exitosamente."
        );
    }

    /// <summary>
    /// Deletes (anula) a sales invoice.
    /// Only invoices in "Completada" status can be cancelled.
    /// This is a soft delete operation that changes the status to "Anulada" and reverses inventory movements.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct = default)
    {
        await _facturaVentaService.DeleteAsync(id, ct);
        return this.NoContentResponse("Factura de venta anulada exitosamente.");
    }
}
