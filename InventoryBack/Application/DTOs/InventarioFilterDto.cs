using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering inventory summary.
/// </summary>
public class InventarioFilterDto
{
    /// <summary>
    /// Filter by warehouse ID (optional)
    /// </summary>
    [JsonPropertyName("bodegaId")]
    public Guid? BodegaId { get; set; }
    
    /// <summary>
    /// Filter by category ID (optional)
    /// </summary>
    [JsonPropertyName("categoriaId")]
    public Guid? CategoriaId { get; set; }
    
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
}
