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
    /// List of products with their inventory details
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
}
