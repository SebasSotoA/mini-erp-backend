using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Product-specific repository interface.
/// </summary>
public interface IProductRepository : IGenericRepository<Producto>
{
    /// <summary>
    /// Gets a product by its SKU code.
    /// </summary>
    /// <param name="sku">SKU code</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product or null if not found</returns>
    Task<Producto?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>
    /// Gets a paginated list of products with advanced filtering.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple with products and total count</returns>
    Task<(IEnumerable<Producto> Items, int TotalCount)> GetPagedAsync(
        ProductFilterDto filters,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a SKU already exists in the database.
    /// </summary>
    /// <param name="sku">SKU to check</param>
    /// <param name="excludeId">Optional ID to exclude from check (for updates)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if SKU exists</returns>
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product is referenced in any invoice.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if product is referenced</returns>
    Task<bool> IsProductReferencedAsync(Guid productId, CancellationToken ct = default);
}
