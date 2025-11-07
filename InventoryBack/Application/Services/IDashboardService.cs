using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for dashboard analytics and charts.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets main dashboard metrics (cards).
    /// </summary>
    Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets top 10 best-selling products.
    /// </summary>
    /// <param name="top">Number of products to return (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<TopProductoVendidoDto>> GetTopProductosVendidosAsync(int top = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets sales trend grouped by date.
    /// </summary>
    /// <param name="dias">Number of days to analyze (default: 30)</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<TendenciaVentasDto>> GetTendenciaVentasAsync(int dias = 30, CancellationToken ct = default);

    /// <summary>
    /// Gets inventory distribution by category.
    /// </summary>
    Task<IEnumerable<DistribucionCategoriasDto>> GetDistribucionCategoriasAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets stock movements (entries vs exits) for the last N days.
    /// </summary>
    /// <param name="dias">Number of days to analyze (default: 30)</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<MovimientosStockDto>> GetMovimientosStockAsync(int dias = 30, CancellationToken ct = default);

    /// <summary>
    /// Gets stock comparison by warehouse.
    /// </summary>
    Task<IEnumerable<StockPorBodegaDto>> GetStockPorBodegaAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets stock health gauge (percentage of products in optimal stock).
    /// </summary>
    Task<SaludStockDto> GetSaludStockAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets list of products with low stock.
    /// </summary>
    /// <param name="top">Number of products to return (default: 20)</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<ProductoStockBajoDto>> GetProductosStockBajoAsync(int top = 20, CancellationToken ct = default);
}
