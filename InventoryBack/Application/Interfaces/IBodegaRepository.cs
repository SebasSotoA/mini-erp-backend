using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

public interface IBodegaRepository : IGenericRepository<Bodega>
{
    Task<Bodega?> GetByNameAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<Bodega>> GetActiveBodegasAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of bodegas with filtering and sorting.
    /// </summary>
    Task<(IEnumerable<Bodega> Items, int TotalCount)> GetPagedAsync(
        BodegaFilterDto filters,
        CancellationToken ct = default);
    
    Task<bool> HasProductsAsync(Guid bodegaId, CancellationToken ct = default);
    Task<bool> HasInvoicesAsync(Guid bodegaId, CancellationToken ct = default);
    Task<bool> HasMovementsAsync(Guid bodegaId, CancellationToken ct = default);
}
