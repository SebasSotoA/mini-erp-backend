using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for individual product in inventory summary.
/// </summary>
public class InventarioProductoDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("codigoSku")]
    public string CodigoSku { get; set; } = string.Empty;
    
    [JsonPropertyName("bodega")]
    public string Bodega { get; set; } = string.Empty;
    
    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }
    
    [JsonPropertyName("costoUnitario")]
    public decimal CostoUnitario { get; set; }
    
    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
    
    /// <summary>
    /// Optional: Category name for grouping
    /// </summary>
    [JsonPropertyName("categoria")]
    public string? Categoria { get; set; }
}
