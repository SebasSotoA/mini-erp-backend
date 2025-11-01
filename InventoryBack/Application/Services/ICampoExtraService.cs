using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

public interface ICampoExtraService
{
    Task<CampoExtraDto> CreateAsync(CreateCampoExtraDto dto, CancellationToken ct = default);
    Task<CampoExtraDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CampoExtraDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateCampoExtraDto dto, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default);
}
