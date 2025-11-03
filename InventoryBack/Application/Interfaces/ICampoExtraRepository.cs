using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for CampoExtra with specific queries.
/// </summary>
public interface ICampoExtraRepository : IGenericRepository<CampoExtra>
{
    /// <summary>
    /// Gets a CampoExtra by its name (case-insensitive).
    /// </summary>
    /// <param name="nombre">Campo name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>CampoExtra or null if not found</returns>
    Task<CampoExtra?> GetByNameAsync(string nombre, CancellationToken ct = default);

    /// <summary>
    /// Gets all active CamposExtra.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active campos extra</returns>
    Task<IEnumerable<CampoExtra>> GetActiveCamposAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a paginated list of campos extra with filtering and sorting.
    /// </summary>
    Task<(IEnumerable<CampoExtra> Items, int TotalCount)> GetPagedAsync(
        CampoExtraFilterDto filters,
        CancellationToken ct = default);
}
