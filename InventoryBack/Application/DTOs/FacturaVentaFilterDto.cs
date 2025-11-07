using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for filtering sales invoices (Facturas de Venta) with search, sorting, and pagination.
/// </summary>
public class FacturaVentaFilterDto
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
    /// Filter by vendedor ID
    /// </summary>
    [JsonPropertyName("vendedorId")]
    public Guid? VendedorId { get; set; }

    /// <summary>
    /// Search by vendedor name (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("vendedorNombre")]
    public string? VendedorNombre { get; set; }

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
    /// Filter by forma de pago: "Contado" or "Credito"
    /// </summary>
    [JsonPropertyName("formaPago")]
    public string? FormaPago { get; set; }

    /// <summary>
    /// Filter by medio de pago: "Efectivo", "Tarjeta", "Transferencia", "Cheque"
    /// </summary>
    [JsonPropertyName("medioPago")]
    public string? MedioPago { get; set; }

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
    /// Sort by field: "numero" (default), "vendedor", "bodega", "fecha", "estado", "total", "formaPago"
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort descending (default: true - newest first)
    /// </summary>
    [JsonPropertyName("orderDesc")]
    public bool OrderDesc { get; set; } = true;
}
