using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Service interface for inventory summary and reporting.
/// </summary>
public interface IInventarioService
{
    /// <summary>
    /// Gets an inventory summary with optional filters.
    /// Calculates total value (stock * cost) and total stock.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory summary with products and totals</returns>
    Task<InventarioResumenDto> GetResumenAsync(InventarioFilterDto filters, CancellationToken ct = default);
}
