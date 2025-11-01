using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

public class UpdateCategoriaDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    
    [JsonPropertyName("imagenCategoriaUrl")]
    public string? ImagenCategoriaUrl { get; set; }
}
