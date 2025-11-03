using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for Proveedor entity.
/// </summary>
public interface IProveedorRepository : IGenericRepository<Proveedor>
{
    /// <summary>
    /// Checks if a proveedor's identificacion already exists.
    /// </summary>
    Task<bool> IdentificacionExistsAsync(string identificacion, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a proveedor has purchase invoices.
    /// </summary>
    Task<bool> HasPurchaseInvoicesAsync(Guid proveedorId, CancellationToken ct = default);

    /// <summary>
    /// Gets proveedor with purchase count.
    /// </summary>
    Task<(Proveedor? Proveedor, int PurchaseCount)> GetWithPurchaseCountAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a proveedor by identificacion.
    /// </summary>
    Task<Proveedor?> GetByIdentificacionAsync(string identificacion, CancellationToken ct = default);
}
