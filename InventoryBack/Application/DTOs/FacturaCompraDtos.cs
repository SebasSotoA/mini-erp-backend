using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a purchase invoice (Factura de Compra).
/// </summary>
public class CreateFacturaCompraDto
{
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("proveedorId")]
    public Guid ProveedorId { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("items")]
    public List<CreateFacturaCompraDetalleDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for purchase invoice line item.
/// </summary>
public class CreateFacturaCompraDetalleDto
{
    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("costoUnitario")]
    public decimal CostoUnitario { get; set; }

    [JsonPropertyName("descuento")]
    public decimal? Descuento { get; set; }

    [JsonPropertyName("impuesto")]
    public decimal? Impuesto { get; set; }
}

/// <summary>
/// DTO for purchase invoice response.
/// </summary>
public class FacturaCompraDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("numeroFactura")]
    public string NumeroFactura { get; set; } = string.Empty;

    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("bodegaNombre")]
    public string? BodegaNombre { get; set; }

    [JsonPropertyName("proveedorId")]
    public Guid ProveedorId { get; set; }

    [JsonPropertyName("proveedorNombre")]
    public string? ProveedorNombre { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("items")]
    public List<FacturaCompraDetalleDto>? Items { get; set; }
}

/// <summary>
/// DTO for purchase invoice detail response.
/// </summary>
public class FacturaCompraDetalleDto
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

    [JsonPropertyName("costoUnitario")]
    public decimal CostoUnitario { get; set; }

    [JsonPropertyName("descuento")]
    public decimal? Descuento { get; set; }

    [JsonPropertyName("impuesto")]
    public decimal? Impuesto { get; set; }

    [JsonPropertyName("totalLinea")]
    public decimal TotalLinea { get; set; }
}
