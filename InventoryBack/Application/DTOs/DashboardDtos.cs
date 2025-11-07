using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

// ========== MÉTRICAS PRINCIPALES (CARDS) ==========

/// <summary>
/// DTO for main dashboard metrics (cards).
/// </summary>
public class DashboardMetricsDto
{
    [JsonPropertyName("totalProductos")]
    public int TotalProductos { get; set; }

    [JsonPropertyName("totalBodegas")]
    public int TotalBodegas { get; set; }

    [JsonPropertyName("ventasDelMes")]
    public decimal VentasDelMes { get; set; }

    [JsonPropertyName("comprasDelMes")]
    public decimal ComprasDelMes { get; set; }

    [JsonPropertyName("productosStockBajo")]
    public int ProductosStockBajo { get; set; }

    [JsonPropertyName("valorTotalInventario")]
    public decimal ValorTotalInventario { get; set; }

    [JsonPropertyName("margenBruto")]
    public decimal MargenBruto { get; set; }

    [JsonPropertyName("porcentajeMargen")]
    public decimal PorcentajeMargen { get; set; }
}

// ========== TOP 10 PRODUCTOS MÁS VENDIDOS ==========

/// <summary>
/// DTO for top selling products chart.
/// </summary>
public class TopProductoVendidoDto
{
    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("productoNombre")]
    public string ProductoNombre { get; set; } = string.Empty;

    [JsonPropertyName("productoSku")]
    public string? ProductoSku { get; set; }

    [JsonPropertyName("cantidadVendida")]
    public int CantidadVendida { get; set; }

    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
}

// ========== TENDENCIA DE VENTAS ==========

/// <summary>
/// DTO for sales trend chart (grouped by date).
/// </summary>
public class TendenciaVentasDto
{
    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("totalVentas")]
    public decimal TotalVentas { get; set; }

    [JsonPropertyName("cantidadFacturas")]
    public int CantidadFacturas { get; set; }
}

// ========== DISTRIBUCIÓN DE INVENTARIO POR CATEGORÍA ==========

/// <summary>
/// DTO for inventory distribution by category chart.
/// </summary>
public class DistribucionCategoriasDto
{
    [JsonPropertyName("categoriaId")]
    public Guid? CategoriaId { get; set; }

    [JsonPropertyName("categoriaNombre")]
    public string CategoriaNombre { get; set; } = string.Empty;

    [JsonPropertyName("cantidadProductos")]
    public int CantidadProductos { get; set; }

    [JsonPropertyName("stockTotal")]
    public int StockTotal { get; set; }

    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
}

// ========== MOVIMIENTOS DE STOCK (ENTRADAS VS SALIDAS) ==========

/// <summary>
/// DTO for stock movements chart (entries vs exits).
/// </summary>
public class MovimientosStockDto
{
    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("entradas")]
    public int Entradas { get; set; }

    [JsonPropertyName("salidas")]
    public int Salidas { get; set; }

    [JsonPropertyName("neto")]
    public int Neto { get; set; }
}

// ========== COMPARACIÓN STOCK POR BODEGA ==========

/// <summary>
/// DTO for warehouse stock comparison chart.
/// </summary>
public class StockPorBodegaDto
{
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }

    [JsonPropertyName("bodegaNombre")]
    public string BodegaNombre { get; set; } = string.Empty;

    [JsonPropertyName("cantidadProductos")]
    public int CantidadProductos { get; set; }

    [JsonPropertyName("stockTotal")]
    public int StockTotal { get; set; }

    [JsonPropertyName("valorTotal")]
    public decimal ValorTotal { get; set; }
}

// ========== GAUGE DE SALUD DEL STOCK ==========

/// <summary>
/// DTO for stock health gauge.
/// </summary>
public class SaludStockDto
{
    [JsonPropertyName("productosStockOptimo")]
    public int ProductosStockOptimo { get; set; }

    [JsonPropertyName("productosStockBajo")]
    public int ProductosStockBajo { get; set; }

    [JsonPropertyName("productosStockAlto")]
    public int ProductosStockAlto { get; set; }

    [JsonPropertyName("productosAgotados")]
    public int ProductosAgotados { get; set; }

    [JsonPropertyName("totalProductos")]
    public int TotalProductos { get; set; }

    [JsonPropertyName("porcentajeStockOptimo")]
    public decimal PorcentajeStockOptimo { get; set; }

    [JsonPropertyName("porcentajeStockBajo")]
    public decimal PorcentajeStockBajo { get; set; }

    [JsonPropertyName("porcentajeStockAlto")]
    public decimal PorcentajeStockAlto { get; set; }

    [JsonPropertyName("porcentajeAgotados")]
    public decimal PorcentajeAgotados { get; set; }
}

// ========== PRODUCTOS CON STOCK BAJO ==========

/// <summary>
/// DTO for products with low stock.
/// </summary>
public class ProductoStockBajoDto
{
    [JsonPropertyName("productoId")]
    public Guid ProductoId { get; set; }

    [JsonPropertyName("productoNombre")]
    public string ProductoNombre { get; set; } = string.Empty;

    [JsonPropertyName("productoSku")]
    public string? ProductoSku { get; set; }

    [JsonPropertyName("stockActual")]
    public int StockActual { get; set; }

    [JsonPropertyName("stockMinimo")]
    public int StockMinimo { get; set; }

    [JsonPropertyName("diferencia")]
    public int Diferencia { get; set; }

    [JsonPropertyName("bodegaPrincipal")]
    public string? BodegaPrincipal { get; set; }
}
