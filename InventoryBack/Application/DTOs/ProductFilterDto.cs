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
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    // ========== GENERAL SEARCH ==========
    
    /// <summary>
    /// General search query (searches in Name, SKU, Description)
    /// </summary>
    public string? Q { get; set; }

    // ========== SPECIFIC FILTERS ==========
    
    /// <summary>
    /// Filter by product name (partial match)
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Filter by SKU code (partial match)
    /// </summary>
    public string? CodigoSku { get; set; }

    /// <summary>
    /// Filter by description (partial match)
    /// </summary>
    public string? Descripcion { get; set; }

    // ========== PRICE FILTER (UPDATED) ==========
    
    /// <summary>
    /// Filter by price (partial match - searches price as text)
    /// Example: "1500" finds 1500000, 2500000, 150000, etc.
    /// </summary>
    public string? Precio { get; set; }

    // ========== QUANTITY FILTERS ==========
    // Note: These filter on ProductoBodega.CantidadInicial
    
    /// <summary>
    /// Exact quantity filter
    /// </summary>
    public int? CantidadExacta { get; set; }

    /// <summary>
    /// Minimum quantity filter (inclusive)
    /// </summary>
    public int? CantidadMin { get; set; }

    /// <summary>
    /// Maximum quantity filter (inclusive)
    /// </summary>
    public int? CantidadMax { get; set; }

    /// <summary>
    /// Quantity comparison operator: "=" (default), ">", ">=", "<", "<="
    /// </summary>
    public string? CantidadOperador { get; set; }

    // ========== STATUS FILTERS ==========
    
    /// <summary>
    /// Include inactive products (default: false)
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Return only inactive products (default: false)
    /// </summary>
    public bool OnlyInactive { get; set; } = false;

    // ========== SORTING ==========
    
    /// <summary>
    /// Sort by field: "nombre" (default), "precio", "sku", "fecha"
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort descending (default: false - ascending)
    /// </summary>
    public bool OrderDesc { get; set; } = false;
}
