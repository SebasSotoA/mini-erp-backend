using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

/// <summary>
/// READ-ONLY controller for inventory movements (audit and traceability).
/// </summary>
[ApiController]
[Route("api/movimientos-inventario")]
[Produces("application/json")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly IMovimientoInventarioService _movimientoService;

    public MovimientosInventarioController(IMovimientoInventarioService movimientoService)
    {
        _movimientoService = movimientoService ?? throw new ArgumentNullException(nameof(movimientoService));
    }

    /// <summary>
    /// Gets a paginated list of inventory movements with advanced filtering.
    /// </summary>
    /// <remarks>
    /// Useful for:
    /// - General audit of all movements
    /// - Filtering by date range, product, warehouse, or movement type
    /// - Dashboard statistics
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<MovimientoInventarioDto>>>> GetAll(
        [FromQuery] MovimientoInventarioFilterDto filters,
        CancellationToken ct = default)
    {
        var result = await _movimientoService.GetPagedAsync(filters, ct);
        return this.OkResponse(result, "Movimientos de inventario obtenidos correctamente.");
    }

    /// <summary>
    /// Gets a specific inventory movement by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MovimientoInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MovimientoInventarioDto>>> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        var movimiento = await _movimientoService.GetByIdAsync(id, ct);
        if (movimiento == null)
        {
            throw new NotFoundException("MovimientoInventario", id);
        }

        return this.OkResponse(movimiento, "Movimiento de inventario obtenido correctamente.");
    }

    /// <summary>
    /// Gets all inventory movements for a specific product (Kardex).
    /// </summary>
    /// <remarks>
    /// Returns the complete movement history for a product across all warehouses.
    /// Useful for generating Kardex reports.
    /// </remarks>
    [HttpGet("producto/{productoId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoInventarioDto>>>> GetByProducto(
        Guid productoId,
        CancellationToken ct = default)
    {
        var movimientos = await _movimientoService.GetByProductoIdAsync(productoId, ct);
        return this.OkResponse(movimientos, "Movimientos del producto obtenidos correctamente.");
    }

    /// <summary>
    /// Gets all inventory movements for a specific warehouse.
    /// </summary>
    /// <remarks>
    /// Returns all movements (entries and exits) for a warehouse.
    /// Useful for warehouse-specific audit reports.
    /// </remarks>
    [HttpGet("bodega/{bodegaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoInventarioDto>>>> GetByBodega(
        Guid bodegaId,
        CancellationToken ct = default)
    {
        var movimientos = await _movimientoService.GetByBodegaIdAsync(bodegaId, ct);
        return this.OkResponse(movimientos, "Movimientos de la bodega obtenidos correctamente.");
    }

    /// <summary>
    /// Gets all inventory movements for a specific sales invoice.
    /// </summary>
    /// <remarks>
    /// Returns all product movements (stock reductions) associated with a sales invoice.
    /// </remarks>
    [HttpGet("factura-venta/{facturaVentaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoInventarioDto>>>> GetByFacturaVenta(
        Guid facturaVentaId,
        CancellationToken ct = default)
    {
        var movimientos = await _movimientoService.GetByFacturaVentaIdAsync(facturaVentaId, ct);
        return this.OkResponse(movimientos, "Movimientos de la factura de venta obtenidos correctamente.");
    }

    /// <summary>
    /// Gets all inventory movements for a specific purchase invoice.
    /// </summary>
    /// <remarks>
    /// Returns all product movements (stock increases) associated with a purchase invoice.
    /// </remarks>
    [HttpGet("factura-compra/{facturaCompraId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MovimientoInventarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoInventarioDto>>>> GetByFacturaCompra(
        Guid facturaCompraId,
        CancellationToken ct = default)
    {
        var movimientos = await _movimientoService.GetByFacturaCompraIdAsync(facturaCompraId, ct);
        return this.OkResponse(movimientos, "Movimientos de la factura de compra obtenidos correctamente.");
    }
}
