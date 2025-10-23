using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service for generating unique SKU codes for products.
/// </summary>
public class SkuGeneratorService : ISkuGeneratorService
{
    private readonly IProductRepository _productRepository;

    public SkuGeneratorService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<string> GenerateSkuAsync(string productName, Guid? categoryId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name cannot be null or empty.", nameof(productName));
        }

        // Generate base SKU from product name (first 3 letters uppercase)
        var namePart = new string(productName
            .Replace(" ", "")
            .Take(3)
            .Select(char.ToUpper)
            .ToArray());

        // Pad with 'X' if name is too short
        namePart = namePart.PadRight(3, 'X');

        // Generate random number part (6 digits)
        var random = new Random();
        var numberPart = random.Next(100000, 999999);

        // Combine parts
        var sku = $"{namePart}-{numberPart}";

        // Ensure uniqueness
        var isUnique = await IsSkuUniqueAsync(sku, null, ct);
        
        // If not unique, recursively generate a new one
        if (!isUnique)
        {
            return await GenerateSkuAsync(productName, categoryId, ct);
        }

        return sku;
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        return !await _productRepository.SkuExistsAsync(sku, excludeProductId, ct);
    }
}
