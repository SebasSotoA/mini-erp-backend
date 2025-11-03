using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

public interface IBodegaService
{
    Task<BodegaDto> CreateAsync(CreateBodegaDto dto, CancellationToken ct = default);
    Task<BodegaDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<BodegaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of bodegas with filtering and sorting.
    /// </summary>
    Task<PagedResult<BodegaDto>> GetPagedAsync(BodegaFilterDto filters, CancellationToken ct = default);
    
    Task UpdateAsync(Guid id, UpdateBodegaDto dto, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default);
}
