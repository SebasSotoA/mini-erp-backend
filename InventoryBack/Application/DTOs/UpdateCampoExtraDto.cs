using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

public class UpdateCampoExtraDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("tipoDato")]
    public string TipoDato { get; set; } = string.Empty;
    
    [JsonPropertyName("esRequerido")]
    public bool EsRequerido { get; set; }
    
    [JsonPropertyName("valorPorDefecto")]
    public string? ValorPorDefecto { get; set; }
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}
