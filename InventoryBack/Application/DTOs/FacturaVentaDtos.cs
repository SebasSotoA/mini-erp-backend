using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a sales invoice (Factura de Venta).
/// </summary>
public class CreateFacturaVentaDto
{
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("vendedorId")]
    public Guid VendedorId { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("formaPago")]
    public string FormaPago { get; set; } = string.Empty; // "Contado" o "Crédito"

    [JsonPropertyName("plazoPago")]
    public int? PlazoPago { get; set; } // Solo si es crédito

    [JsonPropertyName("medioPago")]
    public string MedioPago { get; set; } = string.Empty;

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("items")]
    public List<CreateFacturaVentaDetalleDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for sales invoice line item.
/// </summary>
public class CreateFacturaVentaDetalleDto
{
    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("precioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("descuento")]
    public decimal? Descuento { get; set; }

    [JsonPropertyName("impuesto")]
    public decimal? Impuesto { get; set; }
}

/// <summary>
/// DTO for sales invoice response.
/// </summary>
public class FacturaVentaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("numeroFactura")]
    public string NumeroFactura { get; set; } = string.Empty;

    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("bodegaNombre")]
    public string? BodegaNombre { get; set; }

    [JsonPropertyName("vendedorId")]
    public Guid VendedorId { get; set; }

    [JsonPropertyName("vendedorNombre")]
    public string? VendedorNombre { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("formaPago")]
    public string FormaPago { get; set; } = string.Empty;

    [JsonPropertyName("plazoPago")]
    public int? PlazoPago { get; set; }

    [JsonPropertyName("medioPago")]
    public string MedioPago { get; set; } = string.Empty;

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("items")]
    public List<FacturaVentaDetalleDto>? Items { get; set; }
}

/// <summary>
/// DTO for sales invoice detail response.
/// </summary>
public class FacturaVentaDetalleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("productoNombre")]
    public string? ProductoNombre { get; set; }

    [JsonPropertyName("productoSku")]
    public string? ProductoSku { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("precioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("descuento")]
    public decimal? Descuento { get; set; }

    [JsonPropertyName("impuesto")]
    public decimal? Impuesto { get; set; }

    [JsonPropertyName("totalLinea")]
    public decimal TotalLinea { get; set; }
}
