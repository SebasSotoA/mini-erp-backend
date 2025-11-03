using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for MovimientoInventario with specific queries.
/// This is a READ-ONLY repository for audit and traceability purposes.
/// </summary>
public interface IMovimientoInventarioRepository : IGenericRepository<MovimientoInventario>
{
    /// <summary>
    /// Gets paginated inventory movements with advanced filtering.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple with movements and total count</returns>
    Task<(IEnumerable<MovimientoInventario> Items, int TotalCount)> GetPagedAsync(
        MovimientoInventarioFilterDto filters,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all movements for a specific product.
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventario>> GetByProductoIdAsync(
        Guid productoId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all movements for a specific warehouse.
    /// </summary>
    /// <param name="bodegaId">Warehouse ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventario>> GetByBodegaIdAsync(
        Guid bodegaId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets movements for a specific sales invoice.
    /// </summary>
    /// <param name="facturaVentaId">Sales invoice ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventario>> GetByFacturaVentaIdAsync(
        Guid facturaVentaId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets movements for a specific purchase invoice.
    /// </summary>
    /// <param name="facturaCompraId">Purchase invoice ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventario>> GetByFacturaCompraIdAsync(
        Guid facturaCompraId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets movements within a date range.
    /// </summary>
    /// <param name="fechaDesde">Start date (inclusive)</param>
    /// <param name="fechaHasta">End date (inclusive)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventario>> GetByFechaRangoAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        CancellationToken ct = default);
}
