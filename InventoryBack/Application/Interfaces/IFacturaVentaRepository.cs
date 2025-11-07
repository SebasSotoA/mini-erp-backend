using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for FacturaVenta entity.
/// </summary>
public interface IFacturaVentaRepository : IGenericRepository<FacturaVenta>
{
    /// <summary>
    /// Gets the next invoice number for the current month.
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets invoice by number.
    /// </summary>
    Task<FacturaVenta?> GetByNumeroFacturaAsync(string numeroFactura, CancellationToken ct = default);

    /// <summary>
    /// Gets invoice with details.
    /// </summary>
    Task<FacturaVenta?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all invoice details for a specific invoice.
    /// </summary>
    Task<IEnumerable<FacturaVentaDetalle>> GetDetallesAsync(Guid facturaVentaId, CancellationToken ct = default);

    /// <summary>
    /// Checks if an invoice can be deleted.
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of sales invoices with filtering and sorting.
    /// </summary>
    Task<(IEnumerable<FacturaVenta> Items, int TotalCount)> GetPagedAsync(
        FacturaVentaFilterDto filters,
        CancellationToken ct = default);
}
