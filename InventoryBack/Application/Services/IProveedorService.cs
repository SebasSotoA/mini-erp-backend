using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for Proveedor (supplier) operations.
/// </summary>
public interface IProveedorService
{
    /// <summary>
    /// Creates a new proveedor.
    /// </summary>
    Task<ProveedorDto> CreateAsync(CreateProveedorDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets a proveedor by ID.
    /// </summary>
    Task<ProveedorDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all proveedores (active and inactive).
    /// </summary>
    Task<IEnumerable<ProveedorDto>> GetAllAsync(bool? soloActivos = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of proveedores with filtering and sorting.
    /// </summary>
    Task<PagedResult<ProveedorDto>> GetPagedAsync(ProveedorFilterDto filters, CancellationToken ct = default);

    /// <summary>
    /// Updates a proveedor.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateProveedorDto dto, CancellationToken ct = default);

    /// <summary>
    /// Activates a proveedor.
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a proveedor (soft delete).
    /// </summary>
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
