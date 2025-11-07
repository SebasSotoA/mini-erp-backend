using InventoryBack.Application.DTOs;
using InventoryBack.Application.Services;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

/// <summary>
/// Controller for dashboard analytics and charts.
/// Provides aggregated data for frontend visualizations.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    /// <summary>
    /// Gets main dashboard metrics (cards).
    /// </summary>
    /// <remarks>
    /// Returns key performance indicators:
    /// - Total Products
    /// - Total Warehouses
    /// - Monthly Sales
    /// - Monthly Purchases
    /// - Low Stock Products
    /// - Total Inventory Value
    /// - Gross Margin
    /// </remarks>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ApiResponse<DashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetMetrics(CancellationToken ct = default)
    {
        var metrics = await _dashboardService.GetMetricsAsync(ct);
        return this.OkResponse(metrics, "Métricas del dashboard obtenidas correctamente.");
    }

    /// <summary>
    /// Gets top N best-selling products.
    /// </summary>
    /// <param name="top">Number of products to return (default: 10, max: 50)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("top-productos-vendidos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopProductoVendidoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopProductoVendidoDto>>>> GetTopProductosVendidos(
        [FromQuery] int top = 10,
        CancellationToken ct = default)
    {
        // Validate top parameter
        if (top < 1) top = 10;
        if (top > 50) top = 50;

        var topProductos = await _dashboardService.GetTopProductosVendidosAsync(top, ct);
        return this.OkResponse(topProductos, $"Top {top} productos más vendidos obtenidos correctamente.");
    }

    /// <summary>
    /// Gets sales trend for the last N days.
    /// </summary>
    /// <param name="dias">Number of days to analyze (default: 30, max: 365)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("tendencia-ventas")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TendenciaVentasDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TendenciaVentasDto>>>> GetTendenciaVentas(
        [FromQuery] int dias = 30,
        CancellationToken ct = default)
    {
        // Validate dias parameter
        if (dias < 1) dias = 30;
        if (dias > 365) dias = 365;

        var tendencia = await _dashboardService.GetTendenciaVentasAsync(dias, ct);
        return this.OkResponse(tendencia, $"Tendencia de ventas de los últimos {dias} días obtenida correctamente.");
    }

    /// <summary>
    /// Gets inventory distribution by category.
    /// </summary>
    [HttpGet("distribucion-categorias")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DistribucionCategoriasDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DistribucionCategoriasDto>>>> GetDistribucionCategorias(
        CancellationToken ct = default)
    {
        var distribucion = await _dashboardService.GetDistribucionCategoriasAsync(ct);
        return this.OkResponse(distribucion, "Distribución de inventario por categoría obtenida correctamente.");
    }

    /// <summary>
    /// Gets stock movements (entries vs exits) for the last N days.
    /// </summary>
    /// <param name="dias">Number of days to analyze (default: 30, max: 365)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("movimientos-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MovimientosStockDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovimientosStockDto>>>> GetMovimientosStock(
        [FromQuery] int dias = 30,
        CancellationToken ct = default)
    {
        // Validate dias parameter
        if (dias < 1) dias = 30;
        if (dias > 365) dias = 365;

        var movimientos = await _dashboardService.GetMovimientosStockAsync(dias, ct);
        return this.OkResponse(movimientos, $"Movimientos de stock de los últimos {dias} días obtenidos correctamente.");
    }

    /// <summary>
    /// Gets stock comparison by warehouse.
    /// </summary>
    [HttpGet("stock-por-bodega")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockPorBodegaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockPorBodegaDto>>>> GetStockPorBodega(
        CancellationToken ct = default)
    {
        var stockPorBodega = await _dashboardService.GetStockPorBodegaAsync(ct);
        return this.OkResponse(stockPorBodega, "Comparación de stock por bodega obtenida correctamente.");
    }

    /// <summary>
    /// Gets stock health gauge (percentage of products in optimal stock).
    /// </summary>
    [HttpGet("salud-stock")]
    [ProducesResponseType(typeof(ApiResponse<SaludStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SaludStockDto>>> GetSaludStock(
        CancellationToken ct = default)
    {
        var saludStock = await _dashboardService.GetSaludStockAsync(ct);
        return this.OkResponse(saludStock, "Salud del stock obtenida correctamente.");
    }

    /// <summary>
    /// Gets list of products with low stock.
    /// </summary>
    /// <param name="top">Number of products to return (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("productos-stock-bajo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoStockBajoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductoStockBajoDto>>>> GetProductosStockBajo(
        [FromQuery] int top = 20,
        CancellationToken ct = default)
    {
        // Validate top parameter
        if (top < 1) top = 20;
        if (top > 100) top = 100;

        var productosStockBajo = await _dashboardService.GetProductosStockBajoAsync(top, ct);
        return this.OkResponse(productosStockBajo, $"Top {top} productos con stock bajo obtenidos correctamente.");
    }
}
