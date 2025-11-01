using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering products with multiple criteria.
/// </summary>
public class ProductFilterDto
{
    // ========== PAGINATION ==========
    
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;

    // ========== GENERAL SEARCH ==========
    
    /// <summary>
    /// General search query (searches in Name, SKU, Description)
    /// </summary>
    [JsonPropertyName("q")]
    public string? Q { get; set; }

    // ========== SPECIFIC FILTERS ==========
    
    /// <summary>
    /// Filter by product name (partial match)
    /// </summary>
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Filter by SKU code (partial match)
    /// </summary>
    [JsonPropertyName("codigoSku")]
    public string? CodigoSku { get; set; }

    /// <summary>
    /// Filter by description (partial match)
    /// </summary>
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }

    // ========== PRICE FILTER (UPDATED) ==========
    
    /// <summary>
    /// Filter by price (partial match - searches price as text)
    /// Example: "1500" finds 1500000, 2500000, 150000, etc.
    /// </summary>
    [JsonPropertyName("precio")]
    public string? Precio { get; set; }

    // ========== QUANTITY FILTERS ==========
    // Note: These filter on ProductoBodega.CantidadInicial
    
    /// <summary>
    /// Exact quantity filter
    /// </summary>
    [JsonPropertyName("cantidadExacta")]
    public int? CantidadExacta { get; set; }

    /// <summary>
    /// Minimum quantity filter (inclusive)
    /// </summary>
    [JsonPropertyName("cantidadMin")]
    public int? CantidadMin { get; set; }

    /// <summary>
    /// Maximum quantity filter (inclusive)
    /// </summary>
    [JsonPropertyName("cantidadMax")]
    public int? CantidadMax { get; set; }

    /// <summary>
    /// Quantity comparison operator: "=" (default), ">", ">=", "<", "<="
    /// </summary>
    [JsonPropertyName("cantidadOperador")]
    public string? CantidadOperador { get; set; }

    // ========== STATUS FILTERS ==========
    
    /// <summary>
    /// Include inactive products (default: false)
    /// </summary>
    [JsonPropertyName("includeInactive")]
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Return only inactive products (default: false)
    /// </summary>
    [JsonPropertyName("onlyInactive")]
    public bool OnlyInactive { get; set; } = false;

    // ========== SORTING ==========
    
    /// <summary>
    /// Sort by field: "nombre" (default), "precio", "sku", "fecha"
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort descending (default: false - ascending)
    /// </summary>
    [JsonPropertyName("orderDesc")]
    public bool OrderDesc { get; set; } = false;
}
