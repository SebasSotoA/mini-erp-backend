using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

public class BodegaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("direccion")]
    public string? Direccion { get; set; }
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    
    [JsonPropertyName("activo")]
    public bool Activo { get; set; }
    
    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}
