using System.Linq.Expressions;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists entities with optional filtering.
    /// </summary>
    /// <param name="filter">Optional filter expression</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    /// <param name="entity">Entity to remove</param>
    void Remove(T entity);

    /// <summary>
    /// Counts entities with optional filtering.
    /// </summary>
    /// <param name="filter">Optional filter expression</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default);
}
