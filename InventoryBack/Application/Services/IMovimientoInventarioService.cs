using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for MovimientoInventario (READ-ONLY).
/// Provides audit and traceability for inventory movements.
/// </summary>
public interface IMovimientoInventarioService
{
    /// <summary>
    /// Gets a paginated list of inventory movements with advanced filtering.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated result of movements</returns>
    Task<PagedResult<MovimientoInventarioDto>> GetPagedAsync(
        MovimientoInventarioFilterDto filters,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a specific inventory movement by ID.
    /// </summary>
    /// <param name="id">Movement ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Movement or null if not found</returns>
    Task<MovimientoInventarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all movements for a specific product (Kardex).
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventarioDto>> GetByProductoIdAsync(
        Guid productoId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all movements for a specific warehouse.
    /// </summary>
    /// <param name="bodegaId">Warehouse ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventarioDto>> GetByBodegaIdAsync(
        Guid bodegaId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets movements for a specific sales invoice.
    /// </summary>
    /// <param name="facturaVentaId">Sales invoice ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventarioDto>> GetByFacturaVentaIdAsync(
        Guid facturaVentaId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets movements for a specific purchase invoice.
    /// </summary>
    /// <param name="facturaCompraId">Purchase invoice ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of movements</returns>
    Task<IEnumerable<MovimientoInventarioDto>> GetByFacturaCompraIdAsync(
        Guid facturaCompraId,
        CancellationToken ct = default);
}
