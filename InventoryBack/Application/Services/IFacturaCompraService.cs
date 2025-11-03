using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for purchase invoice (Factura de Compra) operations.
/// </summary>
public interface IFacturaCompraService
{
    /// <summary>
    /// Creates a new purchase invoice.
    /// </summary>
    Task<FacturaCompraDto> CreateAsync(CreateFacturaCompraDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets a purchase invoice by ID with all details.
    /// </summary>
    Task<FacturaCompraDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all purchase invoices.
    /// </summary>
    Task<IEnumerable<FacturaCompraDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Cancels (anula) a purchase invoice using soft delete.
    /// Only invoices in "Completada" status can be cancelled.
    /// Changes status to "Anulada" and reverses inventory movements.
    /// Physical deletion is NOT allowed to maintain audit trail.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
