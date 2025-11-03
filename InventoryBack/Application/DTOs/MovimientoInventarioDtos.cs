using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for inventory movement response.
/// </summary>
public class MovimientoInventarioDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("productoNombre")]
    public string? ProductoNombre { get; set; }

    [JsonPropertyName("productoSku")]
    public string? ProductoSku { get; set; }

    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("bodegaNombre")]
    public string? BodegaNombre { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("tipoMovimiento")]
    public string TipoMovimiento { get; set; } = string.Empty;

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("costoUnitario")]
    public decimal? CostoUnitario { get; set; }

    [JsonPropertyName("precioUnitario")]
    public decimal? PrecioUnitario { get; set; }

    [JsonPropertyName("observacion")]
    public string? Observacion { get; set; }

    [JsonPropertyName("facturaVentaId")]
    public Guid? FacturaVentaId { get; set; }

    [JsonPropertyName("facturaVentaNumero")]
    public string? FacturaVentaNumero { get; set; }

    [JsonPropertyName("facturaCompraId")]
    public Guid? FacturaCompraId { get; set; }

    [JsonPropertyName("facturaCompraNumero")]
    public string? FacturaCompraNumero { get; set; }
}

/// <summary>
/// DTO for filtering inventory movements.
/// </summary>
public class MovimientoInventarioFilterDto
{
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

    /// <summary>
    /// Filter by product ID
    /// </summary>
    [JsonPropertyName("productoId")]
    public Guid? ProductoId { get; set; }

    /// <summary>
    /// Search by product name (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("productoNombre")]
    public string? ProductoNombre { get; set; }

    /// <summary>
    /// Search by product SKU (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("productoSku")]
    public string? ProductoSku { get; set; }

    /// <summary>
    /// Filter by warehouse ID
    /// </summary>
    [JsonPropertyName("bodegaId")]
    public Guid? BodegaId { get; set; }

    /// <summary>
    /// Search by warehouse name (partial match, case-insensitive)
    /// </summary>
    [JsonPropertyName("bodegaNombre")]
    public string? BodegaNombre { get; set; }

    /// <summary>
    /// Filter by movement type (e.g., "ENTRADA", "SALIDA", "AJUSTE_POSITIVO", "AJUSTE_NEGATIVO")
    /// </summary>
    [JsonPropertyName("tipoMovimiento")]
    public string? TipoMovimiento { get; set; }

    /// <summary>
    /// Filter by invoice type: "COMPRA", "VENTA", or null for all
    /// </summary>
    [JsonPropertyName("tipoFactura")]
    public string? TipoFactura { get; set; }

    /// <summary>
    /// Filter by start date (inclusive)
    /// </summary>
    [JsonPropertyName("fechaDesde")]
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter by end date (inclusive)
    /// </summary>
    [JsonPropertyName("fechaHasta")]
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filter by minimum quantity (inclusive)
    /// </summary>
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }

    /// <summary>
    /// Filter by maximum quantity (inclusive)
    /// </summary>
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }

    /// <summary>
    /// Filter by sales invoice ID
    /// </summary>
    [JsonPropertyName("facturaVentaId")]
    public Guid? FacturaVentaId { get; set; }

    /// <summary>
    /// Filter by purchase invoice ID
    /// </summary>
    [JsonPropertyName("facturaCompraId")]
    public Guid? FacturaCompraId { get; set; }

    /// <summary>
    /// Order by field: "fecha" (default), "cantidad", "tipoMovimiento", "productoNombre", "bodegaNombre"
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// Order descending (default: true for fecha)
    /// </summary>
    [JsonPropertyName("orderDesc")]
    public bool OrderDesc { get; set; } = true;
}
