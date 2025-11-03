using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for Vendedor entity.
/// </summary>
public interface IVendedorRepository : IGenericRepository<Vendedor>
{
    /// <summary>
    /// Checks if a vendedor's identificacion already exists.
    /// </summary>
    Task<bool> IdentificacionExistsAsync(string identificacion, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a vendedor has sales invoices.
    /// </summary>
    Task<bool> HasSalesInvoicesAsync(Guid vendedorId, CancellationToken ct = default);

    /// <summary>
    /// Gets vendedor with sales count.
    /// </summary>
    Task<(Vendedor? Vendedor, int SalesCount)> GetWithSalesCountAsync(Guid id, CancellationToken ct = default);
}
