using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

[ApiController]
[Route("api/facturas-compra")]
[Produces("application/json")]
public class FacturasCompraController : ControllerBase
{
    private readonly IFacturaCompraService _facturaCompraService;

    public FacturasCompraController(IFacturaCompraService facturaCompraService)
    {
        _facturaCompraService = facturaCompraService ?? throw new ArgumentNullException(nameof(facturaCompraService));
    }

    /// <summary>
    /// Gets all purchase invoices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FacturaCompraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<FacturaCompraDto>>>> GetAll(CancellationToken ct = default)
    {
        var facturas = await _facturaCompraService.GetAllAsync(ct);
        return this.OkResponse(facturas, "Facturas de compra obtenidas correctamente.");
    }

    /// <summary>
    /// Gets a purchase invoice by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FacturaCompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FacturaCompraDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var factura = await _facturaCompraService.GetByIdAsync(id, ct);
        if (factura == null)
        {
            throw new NotFoundException("FacturaCompra", id);
        }

        return this.OkResponse(factura, "Factura de compra obtenida correctamente.");
    }

    /// <summary>
    /// Creates a new purchase invoice.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<FacturaCompraDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FacturaCompraDto>>> Create(
        [FromBody] CreateFacturaCompraDto dto,
        CancellationToken ct = default)
    {
        var factura = await _facturaCompraService.CreateAsync(dto, ct);
        return this.CreatedResponse(
            nameof(GetById),
            new { id = factura.Id },
            factura,
            "Factura de compra creada exitosamente."
        );
    }

    /// <summary>
    /// Deletes (anula) a purchase invoice.
    /// Only invoices in "Completada" status can be cancelled.
    /// This is a soft delete operation that changes the status to "Anulada" and reverses inventory movements.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct = default)
    {
        await _facturaCompraService.DeleteAsync(id, ct);
        return this.NoContentResponse("Factura de compra anulada exitosamente.");
    }
}
