using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a new salesperson (Vendedor).
/// </summary>
public class CreateVendedorDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO for updating a salesperson.
/// </summary>
public class UpdateVendedorDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO for salesperson response.
/// </summary>
public class VendedorDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("activo")]
    public bool Activo { get; set; }

    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}
