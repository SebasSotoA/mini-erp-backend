using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

public class UpdateBodegaDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("direccion")]
    public string? Direccion { get; set; }
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}
