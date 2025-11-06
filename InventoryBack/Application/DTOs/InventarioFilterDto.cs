using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering inventory summary.
/// Supports multiple warehouses and categories.
/// </summary>
public class InventarioFilterDto
{
    /// <summary>
    /// Filter by one or more warehouse IDs (optional)
    /// Example: ?bodegaIds=guid1&bodegaIds=guid2
    /// </summary>
    [JsonPropertyName("bodegaIds")]
    public List<Guid>? BodegaIds { get; set; }
    
    /// <summary>
    /// Filter by one or more category IDs (optional)
    /// Example: ?categoriaIds=guid1&categoriaIds=guid2
    /// </summary>
    [JsonPropertyName("categoriaIds")]
    public List<Guid>? CategoriaIds { get; set; }
    
    /// <summary>
    /// Filter by status: "activo", "inactivo", or "todos" (default: "activo")
    /// </summary>
    [JsonPropertyName("estado")]
    public string? Estado { get; set; } = "activo";
    
    /// <summary>
    /// Search by product name or SKU (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("q")]
    public string? Q { get; set; }
    
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Page size for pagination (default: 50, max: 1000)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 50;
}
