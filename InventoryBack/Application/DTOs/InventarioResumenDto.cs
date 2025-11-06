using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for inventory summary response.
/// </summary>
public class InventarioResumenDto
{
    /// <summary>
    /// Total inventory value (sum of cantidad * costoInicial for all products)
    /// </summary>
    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
    
    /// <summary>
    /// Total stock across all filtered products
    /// </summary>
    [JsonPropertyName("stockTotal")]
    public int StockTotal { get; set; }
    
    /// <summary>
    /// List of products with their inventory details (paginated)
    /// </summary>
    [JsonPropertyName("productos")]
    public List<InventarioProductoDto> Productos { get; set; } = new();
    
    /// <summary>
    /// Filters applied to this summary (optional, for PDF header)
    /// </summary>
    [JsonPropertyName("filtrosAplicados")]
    public Dictionary<string, string>? FiltrosAplicados { get; set; }
    
    /// <summary>
    /// Timestamp when this report was generated
    /// </summary>
    [JsonPropertyName("fechaGeneracion")]
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Current page number (for pagination)
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    /// <summary>
    /// Page size (for pagination)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total count of products matching filters (before pagination)
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
