using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for sales invoice (Factura de Venta) operations.
/// </summary>
public interface IFacturaVentaService
{
    /// <summary>
    /// Creates a new sales invoice.
    /// </summary>
    Task<FacturaVentaDto> CreateAsync(CreateFacturaVentaDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets a sales invoice by ID with all details.
    /// </summary>
    Task<FacturaVentaDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all sales invoices.
    /// </summary>
    Task<IEnumerable<FacturaVentaDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Cancels (anula) a sales invoice using soft delete.
    /// Only invoices in "Completada" status can be cancelled.
    /// Changes status to "Anulada" and reverses inventory movements.
    /// Physical deletion is NOT allowed to maintain audit trail.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
