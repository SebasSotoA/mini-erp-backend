using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a new supplier (Proveedor).
/// </summary>
public class CreateProveedorDto
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
/// DTO for updating a supplier.
/// </summary>
public class UpdateProveedorDto
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
/// DTO for supplier response.
/// </summary>
public class ProveedorDto
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
