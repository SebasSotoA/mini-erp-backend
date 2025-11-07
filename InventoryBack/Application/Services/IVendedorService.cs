using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for Vendedor (salesperson) operations.
/// </summary>
public interface IVendedorService
{
    /// <summary>
    /// Creates a new vendedor.
    /// </summary>
    Task<VendedorDto> CreateAsync(CreateVendedorDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets a vendedor by ID.
    /// </summary>
    Task<VendedorDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all vendedores (active and inactive).
    /// </summary>
    Task<IEnumerable<VendedorDto>> GetAllAsync(bool? soloActivos = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of vendedores with filtering and sorting.
    /// </summary>
    Task<PagedResult<VendedorDto>> GetPagedAsync(VendedorFilterDto filters, CancellationToken ct = default);

    /// <summary>
    /// Updates a vendedor.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateVendedorDto dto, CancellationToken ct = default);

    /// <summary>
    /// Activates a vendedor.
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a vendedor (soft delete).
    /// </summary>
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
