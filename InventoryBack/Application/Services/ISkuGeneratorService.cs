namespace InventoryBack.Application.Services;

/// <summary>
/// Service interface for generating SKU codes for products.
/// </summary>
public interface ISkuGeneratorService
{
    /// <summary>
    /// Generates a unique SKU code for a product.
    /// </summary>
    /// <param name="productName">Product name</param>
    /// <param name="categoryId">Category ID (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Generated SKU code</returns>
    Task<string> GenerateSkuAsync(string productName, Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>
    /// Validates if a SKU code is unique.
    /// </summary>
    /// <param name="sku">SKU code to validate</param>
    /// <param name="excludeProductId">Product ID to exclude from validation (for updates)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if SKU is unique, false otherwise</returns>
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken ct = default);
}
