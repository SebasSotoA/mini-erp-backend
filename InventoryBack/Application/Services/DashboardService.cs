using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service implementation for dashboard analytics and charts.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        // ? Especificar UTC explícitamente
        var primerDiaMes = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
        var ultimoDiaMes = DateTime.SpecifyKind(primerDiaMes.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

        // 1. Total Productos
        var totalProductos = await _unitOfWork.Products.CountAsync(filter: p => p.Activo, ct: ct);

        // 2. Total Bodegas
        var totalBodegas = await _unitOfWork.Bodegas.CountAsync(filter: b => b.Activo, ct: ct);

        // 3. Ventas del Mes (suma de totales de facturas completadas)
        var facturasVentaMes = await _unitOfWork.FacturasVenta.ListAsync(
            filter: f => f.Estado == "Completada" && f.Fecha >= primerDiaMes && f.Fecha <= ultimoDiaMes,
            ct: ct
        );
        var ventasDelMes = facturasVentaMes.Sum(f => f.Total);

        // 4. Compras del Mes
        var facturasCompraMes = await _unitOfWork.FacturasCompra.ListAsync(
            filter: f => f.Estado == "Completada" && f.Fecha >= primerDiaMes && f.Fecha <= ultimoDiaMes,
            ct: ct
        );
        var comprasDelMes = facturasCompraMes.Sum(f => f.Total);

        // 5. Productos con Stock Bajo
        var productosStockBajo = await GetCountProductosStockBajoAsync(ct);

        // 6. Valor Total Inventario (suma de precioBase * stockActual)
        var valorTotalInventario = await CalcularValorTotalInventarioAsync(ct);

        // 7. Margen Bruto (ventas - compras del mes)
        var margenBruto = ventasDelMes - comprasDelMes;
        var porcentajeMargen = ventasDelMes > 0 ? (margenBruto / ventasDelMes) * 100 : 0;

        return new DashboardMetricsDto
        {
            TotalProductos = totalProductos,
            TotalBodegas = totalBodegas,
            VentasDelMes = ventasDelMes,
            ComprasDelMes = comprasDelMes,
            ProductosStockBajo = productosStockBajo,
            ValorTotalInventario = valorTotalInventario,
            MargenBruto = margenBruto,
            PorcentajeMargen = porcentajeMargen
        };
    }

    public async Task<IEnumerable<TopProductoVendidoDto>> GetTopProductosVendidosAsync(int top = 10, CancellationToken ct = default)
    {
        // Obtener todos los movimientos de tipo VENTA (cantidad positiva)
        var movimientosVenta = await _unitOfWork.MovimientosInventario.ListAsync(
            filter: m => m.TipoMovimiento == "VENTA" && m.Cantidad > 0,
            ct: ct
        );

        // Agrupar por producto y sumar cantidades
        var topProductos = movimientosVenta
            .GroupBy(m => m.ProductoId)
            .Select(g => new
            {
                ProductoId = g.Key,
                CantidadVendida = g.Sum(m => m.Cantidad),
                ValorTotal = g.Sum(m => m.PrecioUnitario.HasValue ? m.Cantidad * m.PrecioUnitario.Value : 0)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(top)
            .ToList();

        // Obtener nombres de productos
        var result = new List<TopProductoVendidoDto>();
        foreach (var item in topProductos)
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(item.ProductoId, ct);
            if (producto != null)
            {
                result.Add(new TopProductoVendidoDto
                {
                    ProductoId = item.ProductoId,
                    ProductoNombre = producto.Nombre,
                    ProductoSku = producto.CodigoSku,
                    CantidadVendida = item.CantidadVendida,
                    ValorTotal = item.ValorTotal
                });
            }
        }

        return result;
    }

    public async Task<IEnumerable<TendenciaVentasDto>> GetTendenciaVentasAsync(int dias = 30, CancellationToken ct = default)
    {
        // ? Crear fecha UTC y luego extraer solo la parte de fecha
        var fechaInicioUtc = DateTime.UtcNow.AddDays(-dias);
        var fechaInicio = DateTime.SpecifyKind(fechaInicioUtc.Date, DateTimeKind.Utc);

        // Obtener facturas de venta completadas en el período
        var facturas = await _unitOfWork.FacturasVenta.ListAsync(
            filter: f => f.Estado == "Completada" && f.Fecha >= fechaInicio,
            ct: ct
        );

        // Agrupar por fecha (solo día, sin hora)
        var tendencia = facturas
            .GroupBy(f => DateTime.SpecifyKind(f.Fecha.Date, DateTimeKind.Utc))
            .Select(g => new TendenciaVentasDto
            {
                Fecha = g.Key,
                TotalVentas = g.Sum(f => f.Total),
                CantidadFacturas = g.Count()
            })
            .OrderBy(x => x.Fecha)
            .ToList();

        return tendencia;
    }

    public async Task<IEnumerable<DistribucionCategoriasDto>> GetDistribucionCategoriasAsync(CancellationToken ct = default)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        var categorias = await _unitOfWork.Categorias.ListAsync(ct: ct);

        // Agrupar productos por categoría
        var distribucion = new List<DistribucionCategoriasDto>();

        // Productos CON categoría
        var productosConCategoria = productos.Where(p => p.CategoriaId.HasValue)
            .GroupBy(p => p.CategoriaId!.Value);

        foreach (var grupo in productosConCategoria)
        {
            var categoria = categorias.FirstOrDefault(c => c.Id == grupo.Key);
            var stockTotal = 0;
            var valorTotal = 0m;

            foreach (var producto in grupo)
            {
                var stock = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
                stockTotal += stock;
                valorTotal += stock * producto.PrecioBase;
            }

            distribucion.Add(new DistribucionCategoriasDto
            {
                CategoriaId = grupo.Key,
                CategoriaNombre = categoria?.Nombre ?? "Desconocida",
                CantidadProductos = grupo.Count(),
                StockTotal = stockTotal,
                ValorTotal = valorTotal
            });
        }

        // Productos SIN categoría
        var productosSinCategoria = productos.Where(p => !p.CategoriaId.HasValue).ToList();
        if (productosSinCategoria.Any())
        {
            var stockTotal = 0;
            var valorTotal = 0m;

            foreach (var producto in productosSinCategoria)
            {
                var stock = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
                stockTotal += stock;
                valorTotal += stock * producto.PrecioBase;
            }

            distribucion.Add(new DistribucionCategoriasDto
            {
                CategoriaId = null,
                CategoriaNombre = "Sin Categoría",
                CantidadProductos = productosSinCategoria.Count,
                StockTotal = stockTotal,
                ValorTotal = valorTotal
            });
        }

        return distribucion.OrderByDescending(d => d.ValorTotal);
    }

    public async Task<IEnumerable<MovimientosStockDto>> GetMovimientosStockAsync(int dias = 30, CancellationToken ct = default)
    {
        // ? Crear fecha UTC
        var fechaInicioUtc = DateTime.UtcNow.AddDays(-dias);
        var fechaInicio = DateTime.SpecifyKind(fechaInicioUtc.Date, DateTimeKind.Utc);

        // Obtener todos los movimientos en el período
        var movimientos = await _unitOfWork.MovimientosInventario.ListAsync(
            filter: m => m.Fecha >= fechaInicio,
            ct: ct
        );

        // Agrupar por fecha
        var movimientosPorDia = movimientos
            .GroupBy(m => DateTime.SpecifyKind(m.Fecha.Date, DateTimeKind.Utc))
            .Select(g => new MovimientosStockDto
            {
                Fecha = g.Key,
                // Entradas: COMPRA con cantidad positiva
                Entradas = g.Where(m => m.TipoMovimiento == "COMPRA" && m.Cantidad > 0).Sum(m => m.Cantidad),
                // Salidas: VENTA con cantidad positiva
                Salidas = g.Where(m => m.TipoMovimiento == "VENTA" && m.Cantidad > 0).Sum(m => m.Cantidad),
                // Neto: diferencia entre entradas y salidas
                Neto = g.Where(m => m.TipoMovimiento == "COMPRA" && m.Cantidad > 0).Sum(m => m.Cantidad) -
                       g.Where(m => m.TipoMovimiento == "VENTA" && m.Cantidad > 0).Sum(m => m.Cantidad)
            })
            .OrderBy(x => x.Fecha)
            .ToList();

        return movimientosPorDia;
    }

    public async Task<IEnumerable<StockPorBodegaDto>> GetStockPorBodegaAsync(CancellationToken ct = default)
    {
        var bodegas = await _unitOfWork.Bodegas.ListAsync(filter: b => b.Activo, ct: ct);
        var productoBodegas = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);

        var resultado = new List<StockPorBodegaDto>();

        foreach (var bodega in bodegas)
        {
            // Obtener todos los ProductoBodega de esta bodega
            var stocksBodega = productoBodegas.Where(pb => pb.BodegaId == bodega.Id).ToList();
            
            var stockTotal = stocksBodega.Sum(pb => pb.StockActual);
            var cantidadProductos = stocksBodega.Count;
            
            // Calcular valor total
            var valorTotal = 0m;
            foreach (var pb in stocksBodega)
            {
                var producto = productos.FirstOrDefault(p => p.Id == pb.ProductoId);
                if (producto != null)
                {
                    valorTotal += pb.StockActual * producto.PrecioBase;
                }
            }

            resultado.Add(new StockPorBodegaDto
            {
                BodegaId = bodega.Id,
                BodegaNombre = bodega.Nombre,
                CantidadProductos = cantidadProductos,
                StockTotal = stockTotal,
                ValorTotal = valorTotal
            });
        }

        return resultado.OrderByDescending(r => r.ValorTotal);
    }

    public async Task<SaludStockDto> GetSaludStockAsync(CancellationToken ct = default)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        var productoBodegas = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);

        var productosOptimo = 0;
        var productosBajo = 0;
        var productosAlto = 0;
        var productosAgotados = 0;

        foreach (var producto in productos)
        {
            var stockTotal = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);

            // Obtener bodega principal para obtener min/max
            var bodegaPrincipal = productoBodegas.FirstOrDefault(pb => 
                pb.ProductoId == producto.Id && pb.BodegaId == producto.BodegaPrincipalId);

            if (stockTotal == 0)
            {
                productosAgotados++;
            }
            else if (bodegaPrincipal != null)
            {
                // Si tiene cantidades mínima y máxima definidas
                if (bodegaPrincipal.CantidadMinima.HasValue && bodegaPrincipal.CantidadMaxima.HasValue)
                {
                    if (stockTotal < bodegaPrincipal.CantidadMinima.Value)
                    {
                        productosBajo++;
                    }
                    else if (stockTotal > bodegaPrincipal.CantidadMaxima.Value)
                    {
                        productosAlto++;
                    }
                    else
                    {
                        productosOptimo++;
                    }
                }
                else if (bodegaPrincipal.CantidadMinima.HasValue)
                {
                    // Solo tiene mínimo
                    if (stockTotal < bodegaPrincipal.CantidadMinima.Value)
                    {
                        productosBajo++;
                    }
                    else
                    {
                        productosOptimo++;
                    }
                }
                else
                {
                    // No tiene límites definidos
                    productosOptimo++;
                }
            }
            else
            {
                // No tiene bodega principal definida
                productosOptimo++;
            }
        }

        var totalProductos = productos.Count();
        var porcentajeOptimo = totalProductos > 0 ? (decimal)productosOptimo / totalProductos * 100 : 0;
        var porcentajeBajo = totalProductos > 0 ? (decimal)productosBajo / totalProductos * 100 : 0;
        var porcentajeAlto = totalProductos > 0 ? (decimal)productosAlto / totalProductos * 100 : 0;
        var porcentajeAgotados = totalProductos > 0 ? (decimal)productosAgotados / totalProductos * 100 : 0;

        return new SaludStockDto
        {
            ProductosStockOptimo = productosOptimo,
            ProductosStockBajo = productosBajo,
            ProductosStockAlto = productosAlto,
            ProductosAgotados = productosAgotados,
            TotalProductos = totalProductos,
            PorcentajeStockOptimo = porcentajeOptimo,
            PorcentajeStockBajo = porcentajeBajo,
            PorcentajeStockAlto = porcentajeAlto,
            PorcentajeAgotados = porcentajeAgotados
        };
    }

    public async Task<IEnumerable<ProductoStockBajoDto>> GetProductosStockBajoAsync(int top = 20, CancellationToken ct = default)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        var productoBodegas = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);
        var bodegas = await _unitOfWork.Bodegas.ListAsync(ct: ct);

        var productosStockBajo = new List<ProductoStockBajoDto>();

        foreach (var producto in productos)
        {
            var stockTotal = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            
            // Obtener bodega principal
            var bodegaPrincipal = productoBodegas.FirstOrDefault(pb => 
                pb.ProductoId == producto.Id && pb.BodegaId == producto.BodegaPrincipalId);

            if (bodegaPrincipal?.CantidadMinima.HasValue == true)
            {
                if (stockTotal < bodegaPrincipal.CantidadMinima.Value)
                {
                    var bodega = bodegas.FirstOrDefault(b => b.Id == producto.BodegaPrincipalId);
                    
                    productosStockBajo.Add(new ProductoStockBajoDto
                    {
                        ProductoId = producto.Id,
                        ProductoNombre = producto.Nombre,
                        ProductoSku = producto.CodigoSku,
                        StockActual = stockTotal,
                        StockMinimo = bodegaPrincipal.CantidadMinima.Value,
                        Diferencia = stockTotal - bodegaPrincipal.CantidadMinima.Value,
                        BodegaPrincipal = bodega?.Nombre
                    });
                }
            }
        }

        return productosStockBajo
            .OrderBy(p => p.Diferencia)
            .Take(top);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private async Task<int> GetCountProductosStockBajoAsync(CancellationToken ct)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        var productoBodegas = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);

        var count = 0;

        foreach (var producto in productos)
        {
            var stockTotal = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            
            var bodegaPrincipal = productoBodegas.FirstOrDefault(pb => 
                pb.ProductoId == producto.Id && pb.BodegaId == producto.BodegaPrincipalId);

            if (bodegaPrincipal?.CantidadMinima.HasValue == true)
            {
                if (stockTotal < bodegaPrincipal.CantidadMinima.Value)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private async Task<decimal> CalcularValorTotalInventarioAsync(CancellationToken ct)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        
        var valorTotal = 0m;

        foreach (var producto in productos)
        {
            var stockTotal = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            valorTotal += stockTotal * producto.PrecioBase;
        }

        return valorTotal;
    }
}
