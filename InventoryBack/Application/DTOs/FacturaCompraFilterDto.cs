using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering purchase invoices (Facturas de Compra) with search, sorting, and pagination.
/// </summary>
public class FacturaCompraFilterDto
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
    /// Search by invoice number (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("numeroFactura")]
    public string? NumeroFactura { get; set; }

    /// <summary>
    /// Filter by proveedor ID
    /// </summary>
    [JsonPropertyName("proveedorId")]
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Search by proveedor name (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("proveedorNombre")]
    public string? ProveedorNombre { get; set; }

    /// <summary>
    /// Filter by bodega ID
    /// </summary>
    [JsonPropertyName("bodegaId")]
    public Guid? BodegaId { get; set; }

    /// <summary>
    /// Search by bodega name (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("bodegaNombre")]
    public string? BodegaNombre { get; set; }

    /// <summary>
    /// Filter by estado: "Completada" or "Anulada"
    /// </summary>
    [JsonPropertyName("estado")]
    public string? Estado { get; set; }

    /// <summary>
    /// Filter from this date (inclusive)
    /// </summary>
    [JsonPropertyName("fechaDesde")]
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter until this date (inclusive)
    /// </summary>
    [JsonPropertyName("fechaHasta")]
    public DateTime? FechaHasta { get; set; }

    // ========== SORTING ==========
    
    /// <summary>
    /// Sort by field: "numero" (default), "proveedor", "bodega", "fecha", "estado", "total"
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort descending (default: true - newest first)
    /// </summary>
    [JsonPropertyName("orderDesc")]
    public bool OrderDesc { get; set; } = true;
}
