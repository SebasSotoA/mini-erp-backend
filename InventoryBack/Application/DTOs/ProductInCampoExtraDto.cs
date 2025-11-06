using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// Extended Product DTO that includes information about extra field assignment.
/// Used for the GET /api/campos-extra/{campoExtraId}/productos endpoint.
/// </summary>
public class ProductInCampoExtraDto : ProductDto
{
    /// <summary>
    /// Total quantity of this product across all warehouses.
    /// Same as StockActual, included for consistency with other endpoints.
    /// </summary>
    [JsonPropertyName("cantidadEnCampo")]
    public int CantidadEnCampo { get; set; }
    
    /// <summary>
    /// Value assigned to this extra field for this specific product.
    /// Example: If field is "Color", value could be "Rojo", "Azul", etc.
    /// </summary>
    [JsonPropertyName("valorCampoExtra")]
    public string? ValorCampoExtra { get; set; }
}
