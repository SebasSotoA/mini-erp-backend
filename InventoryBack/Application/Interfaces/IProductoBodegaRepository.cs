namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Repository interface for ProductoBodega entity with specific queries.
/// </summary>
public interface IProductoBodegaRepository : IGenericRepository<Domain.Entities.ProductoBodega>
{
    /// <summary>
    /// Gets all ProductoBodega entries for a specific product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of ProductoBodega entries</returns>
    Task<IEnumerable<Domain.Entities.ProductoBodega>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets all ProductoBodega entries for a specific warehouse.
    /// </summary>
    /// <param name="bodegaId">Warehouse ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of ProductoBodega entries</returns>
    Task<IEnumerable<Domain.Entities.ProductoBodega>> GetByBodegaIdAsync(Guid bodegaId, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific ProductoBodega entry by product and warehouse IDs.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="bodegaId">Warehouse ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>ProductoBodega entry or null</returns>
    Task<Domain.Entities.ProductoBodega?> GetByProductAndBodegaAsync(Guid productId, Guid bodegaId, CancellationToken ct = default);
}
