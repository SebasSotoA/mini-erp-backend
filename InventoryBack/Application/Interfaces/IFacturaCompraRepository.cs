using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for FacturaCompra entity.
/// </summary>
public interface IFacturaCompraRepository : IGenericRepository<FacturaCompra>
{
    /// <summary>
    /// Gets the next invoice number for the current month.
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets invoice by number.
    /// </summary>
    Task<FacturaCompra?> GetByNumeroFacturaAsync(string numeroFactura, CancellationToken ct = default);

    /// <summary>
    /// Gets invoice with details.
    /// </summary>
    Task<FacturaCompra?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all invoice details for a specific invoice.
    /// </summary>
    Task<IEnumerable<FacturaCompraDetalle>> GetDetallesAsync(Guid facturaCompraId, CancellationToken ct = default);

    /// <summary>
    /// Checks if an invoice can be deleted.
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of purchase invoices with filtering and sorting.
    /// </summary>
    Task<(IEnumerable<FacturaCompra> Items, int TotalCount)> GetPagedAsync(
        FacturaCompraFilterDto filters,
        CancellationToken ct = default);
}
