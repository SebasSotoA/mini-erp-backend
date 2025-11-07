using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering proveedores with search and sorting.
/// </summary>
public class ProveedorFilterDto
{
    // ========== PAGINATION ==========
    
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;

    // ========== SEARCH FILTERS ==========
    
    /// <summary>
    /// Search by nombre (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Search by identificacion (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("identificacion")]
    public string? Identificacion { get; set; }

    /// <summary>
    /// Search by correo (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    // ========== STATUS FILTER ==========
    
    /// <summary>
    /// Filter by status: null = all, true = active only, false = inactive only
    /// </summary>
    [JsonPropertyName("activo")]
    public bool? Activo { get; set; }

    // ========== SORTING ==========
    
    /// <summary>
    /// Sort by field: "nombre" (default), "identificacion", "correo", "fecha"
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort descending (default: false - ascending)
    /// </summary>
    [JsonPropertyName("orderDesc")]
    public bool OrderDesc { get; set; } = false;
}
