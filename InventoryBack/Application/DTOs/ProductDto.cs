using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// Product DTO for API responses.
/// </summary>
public class ProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("unidadMedida")]
    public string UnidadMedida { get; set; } = string.Empty;
    
    [JsonPropertyName("precioBase")]
    public decimal PrecioBase { get; set; }
    
    [JsonPropertyName("impuestoPorcentaje")]
    public decimal? ImpuestoPorcentaje { get; set; }
    
    [JsonPropertyName("costoInicial")]
    public decimal CostoInicial { get; set; }
    
    [JsonPropertyName("categoriaId")]
    public Guid? CategoriaId { get; set; }
    
    [JsonPropertyName("codigoSku")]
    public string? CodigoSku { get; set; }
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    
    [JsonPropertyName("activo")]
    public bool Activo { get; set; }
    
    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
    
    [JsonPropertyName("imagenProductoUrl")]
    public string? ImagenProductoUrl { get; set; }
}
