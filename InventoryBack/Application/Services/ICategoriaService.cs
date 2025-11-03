using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

public interface ICategoriaService
{
    Task<CategoriaDto> CreateAsync(CreateCategoriaDto dto, CancellationToken ct = default);
    Task<CategoriaDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CategoriaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default);
    Task<PagedResult<CategoriaDto>> GetPagedAsync(CategoriaFilterDto filters, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateCategoriaDto dto, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default);
}
