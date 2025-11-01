using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

public class CategoriaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    
    [JsonPropertyName("activo")]
    public bool Activo { get; set; }
    
    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
    
    [JsonPropertyName("imagenCategoriaUrl")]
    public string? ImagenCategoriaUrl { get; set; }
}
