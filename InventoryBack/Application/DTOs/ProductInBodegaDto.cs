using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// Extended Product DTO that includes the quantity in a specific warehouse.
/// Used for the GET /api/bodegas/{bodegaId}/productos endpoint.
/// </summary>
public class ProductInBodegaDto : ProductDto
{
    /// <summary>
    /// Quantity of this product in the specific warehouse being queried.
    /// This is different from StockActual which is the total across ALL warehouses.
    /// </summary>
    [JsonPropertyName("cantidadEnBodega")]
    public int CantidadEnBodega { get; set; }
}
