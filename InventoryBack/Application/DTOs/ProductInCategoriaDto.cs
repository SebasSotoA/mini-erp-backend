using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// Extended Product DTO that includes the total quantity in a specific category.
/// Used for the GET /api/categorias/{categoriaId}/productos endpoint.
/// </summary>
public class ProductInCategoriaDto : ProductDto
{
    /// <summary>
    /// Total quantity of this product across all warehouses.
    /// Since a product belongs to only ONE category, this equals StockActual.
    /// Included for consistency with ProductInBodegaDto API design.
    /// </summary>
    [JsonPropertyName("cantidadEnCategoria")]
    public int CantidadEnCategoria { get; set; }
}
