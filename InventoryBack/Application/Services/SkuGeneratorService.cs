using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service for generating unique SKU codes for products.
/// Format: LLL-PPP-001
/// - LLL: First 3 letters of product name
/// - PPP: First 3 letters of category name (or "GEN" for General if no category)
/// - 001: Sequential number (incremental)
/// </summary>
public class SkuGeneratorService : ISkuGeneratorService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public SkuGeneratorService(
        IProductRepository productRepository,
        ICategoriaRepository categoriaRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
    }

    public async Task<string> GenerateSkuAsync(string productName, Guid? categoryId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name cannot be null or empty.", nameof(productName));
        }

        // ========== 1. GENERATE NAME PART (LLL) ==========
        var namePart = GenerateNamePart(productName);

        // ========== 2. GENERATE CATEGORY PART (PPP) ==========
        var categoryPart = await GenerateCategoryPartAsync(categoryId, ct);

        // ========== 3. GENERATE SEQUENTIAL NUMBER (001) ==========
        var numberPart = await GenerateSequentialNumberAsync(namePart, categoryPart, ct);

        // ========== 4. COMBINE PARTS ==========
        var sku = $"{namePart}-{categoryPart}-{numberPart}";

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

    // ========== PRIVATE HELPER METHODS ==========

    /// <summary>
    /// Generates the name part (LLL) from product name.
    /// Takes first 3 letters, uppercase, removes spaces and special characters.
    /// Pads with 'X' if name is too short.
    /// </summary>
    private string GenerateNamePart(string productName)
    {
        // Remove spaces, special characters, and take only letters
        var cleanName = new string(productName
            .Where(c => char.IsLetter(c))
            .Take(3)
            .Select(char.ToUpper)
            .ToArray());

        // Pad with 'X' if too short
        return cleanName.PadRight(3, 'X');
    }

    /// <summary>
    /// Generates the category part (PPP) from category name.
    /// If no category, uses "GEN" (General).
    /// </summary>
    private async Task<string> GenerateCategoryPartAsync(Guid? categoryId, CancellationToken ct)
    {
        if (!categoryId.HasValue)
        {
            return "GEN"; // Default for products without category
        }

        var category = await _categoriaRepository.GetByIdAsync(categoryId.Value, ct);
        if (category == null)
        {
            return "GEN"; // Fallback if category not found
        }

        // Extract first 3 letters from category name
        var categoryPart = new string(category.Nombre
            .Where(c => char.IsLetter(c))
            .Take(3)
            .Select(char.ToUpper)
            .ToArray());

        // Pad with 'X' if too short
        return categoryPart.PadRight(3, 'X');
    }

    /// <summary>
    /// Generates the sequential number part (001, 002, 003...).
    /// Finds the highest existing number for the given prefix and increments.
    /// </summary>
    private async Task<string> GenerateSequentialNumberAsync(
        string namePart, 
        string categoryPart, 
        CancellationToken ct)
    {
        // Get all products to find existing SKUs with same prefix
        var allProducts = await _productRepository.ListAsync(ct: ct);
        
        var prefix = $"{namePart}-{categoryPart}-";
        
        // Find all SKUs with the same prefix
        var existingNumbers = allProducts
            .Where(p => !string.IsNullOrEmpty(p.CodigoSku) && p.CodigoSku.StartsWith(prefix))
            .Select(p => p.CodigoSku)
            .Select(sku =>
            {
                // Extract the number part (last 3 digits)
                var parts = sku.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var number))
                {
                    return number;
                }
                return 0;
            })
            .Where(n => n > 0)
            .ToList();

        // Find the next number
        var nextNumber = existingNumbers.Any() 
            ? existingNumbers.Max() + 1 
            : 1;

        // Format as 3-digit number with leading zeros
        return nextNumber.ToString("D3");
    }
}
