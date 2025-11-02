using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for adding a product to a warehouse.
/// </summary>
public class AddProductoBodegaDto
{
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }
    
    [JsonPropertyName("cantidadInicial")]
    public int CantidadInicial { get; set; }
    
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }
    
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }
}

/// <summary>
/// DTO for updating product quantities in a warehouse.
/// </summary>
public class UpdateProductoBodegaDto
{
    [JsonPropertyName("cantidadInicial")]
    public int CantidadInicial { get; set; }
    
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }
    
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }
}

/// <summary>
/// DTO for product-warehouse details (response).
/// </summary>
public class ProductoBodegaDetailDto
{
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }
    
    [JsonPropertyName("bodegaNombre")]
    public string BodegaNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("bodegaDireccion")]
    public string? BodegaDireccion { get; set; }
    
    [JsonPropertyName("cantidadInicial")]
    public int CantidadInicial { get; set; }
    
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }
    
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }
}

/// <summary>
/// DTO for setting a product's extra field value.
/// </summary>
public class SetProductoCampoExtraDto
{
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;
}

/// <summary>
/// DTO for product-campo extra details (response).
/// </summary>
public class ProductoCampoExtraDetailDto
{
    [JsonPropertyName("campoExtraId")]
    public Guid CampoExtraId { get; set; }
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("tipoDato")]
    public string TipoDato { get; set; } = string.Empty;
    
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;
    
    [JsonPropertyName("esRequerido")]
    public bool EsRequerido { get; set; }
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}
