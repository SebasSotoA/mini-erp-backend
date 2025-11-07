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

        // 6. Valor Total Inventario (suma de costoInicial * stockActual)
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
                valorTotal += stock * producto.CostoInicial;
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
                valorTotal += stock * producto.CostoInicial;
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
                    valorTotal += pb.StockActual * producto.CostoInicial;
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

        // Usamos HashSet para evitar contar el mismo producto múltiples veces
        var productosOptimo = new HashSet<Guid>();
        var productosBajo = new HashSet<Guid>();
        var productosAlto = new HashSet<Guid>();
        var productosAgotados = new HashSet<Guid>();

        foreach (var producto in productos)
        {
            // Obtener todas las relaciones ProductoBodega para este producto
            var relacionesBodegas = productoBodegas.Where(pb => pb.ProductoId == producto.Id).ToList();

            if (!relacionesBodegas.Any())
            {
                // No tiene bodegas asignadas - considerarlo óptimo por defecto
                productosOptimo.Add(producto.Id);
                continue;
            }

            // Verificar el stock en cada bodega donde existe el producto
            var tieneStockBajo = false;
            var tieneStockAlto = false;
            var tieneStockOptimo = false;
            var estaAgotado = false;

            foreach (var relacion in relacionesBodegas)
            {
                if (relacion.StockActual == 0)
                {
                    estaAgotado = true;
                }
                else if (relacion.CantidadMinima.HasValue && relacion.CantidadMaxima.HasValue)
                {
                    // Tiene ambos límites definidos
                    if (relacion.StockActual < relacion.CantidadMinima.Value)
                    {
                        tieneStockBajo = true;
                    }
                    else if (relacion.StockActual > relacion.CantidadMaxima.Value)
                    {
                        tieneStockAlto = true;
                    }
                    else
                    {
                        tieneStockOptimo = true;
                    }
                }
                else if (relacion.CantidadMinima.HasValue)
                {
                    // Solo tiene mínimo
                    if (relacion.StockActual < relacion.CantidadMinima.Value)
                    {
                        tieneStockBajo = true;
                    }
                    else
                    {
                        tieneStockOptimo = true;
                    }
                }
                else if (relacion.CantidadMaxima.HasValue)
                {
                    // Solo tiene máximo
                    if (relacion.StockActual > relacion.CantidadMaxima.Value)
                    {
                        tieneStockAlto = true;
                    }
                    else
                    {
                        tieneStockOptimo = true;
                    }
                }
                else
                {
                    // No tiene límites definidos en esta bodega
                    if (relacion.StockActual > 0)
                    {
                        tieneStockOptimo = true;
                    }
                }
            }

            // Clasificar el producto según el peor caso encontrado
            // Prioridad: Agotado > Bajo > Alto > Óptimo
            if (estaAgotado && relacionesBodegas.All(r => r.StockActual == 0))
            {
                // Solo está agotado si TODAS las bodegas tienen stock 0
                productosAgotados.Add(producto.Id);
            }
            else if (tieneStockBajo)
            {
                // Tiene al menos una bodega con stock bajo
                productosBajo.Add(producto.Id);
            }
            else if (tieneStockAlto)
            {
                // Tiene al menos una bodega con stock alto (y ninguna con stock bajo)
                productosAlto.Add(producto.Id);
            }
            else if (tieneStockOptimo)
            {
                // Todas las bodegas están en nivel óptimo
                productosOptimo.Add(producto.Id);
            }
            else
            {
                // No tiene límites definidos en ninguna bodega
                productosOptimo.Add(producto.Id);
            }
        }

        var totalProductos = productos.Count();
        var porcentajeOptimo = totalProductos > 0 ? (decimal)productosOptimo.Count / totalProductos * 100 : 0;
        var porcentajeBajo = totalProductos > 0 ? (decimal)productosBajo.Count / totalProductos * 100 : 0;
        var porcentajeAlto = totalProductos > 0 ? (decimal)productosAlto.Count / totalProductos * 100 : 0;
        var porcentajeAgotados = totalProductos > 0 ? (decimal)productosAgotados.Count / totalProductos * 100 : 0;

        return new SaludStockDto
        {
            ProductosStockOptimo = productosOptimo.Count,
            ProductosStockBajo = productosBajo.Count,
            ProductosStockAlto = productosAlto.Count,
            ProductosAgotados = productosAgotados.Count,
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
            // Obtener todas las relaciones ProductoBodega para este producto
            var relacionesBodegas = productoBodegas.Where(pb => pb.ProductoId == producto.Id).ToList();

            foreach (var relacion in relacionesBodegas)
            {
                // Solo agregar si tiene CantidadMinima definida y el stock está por debajo
                if (relacion.CantidadMinima.HasValue && relacion.StockActual < relacion.CantidadMinima.Value)
                {
                    var bodega = bodegas.FirstOrDefault(b => b.Id == relacion.BodegaId);
                    
                    productosStockBajo.Add(new ProductoStockBajoDto
                    {
                        ProductoId = producto.Id,
                        ProductoNombre = producto.Nombre,
                        ProductoSku = producto.CodigoSku,
                        StockActual = relacion.StockActual,
                        StockMinimo = relacion.CantidadMinima.Value,
                        Diferencia = relacion.StockActual - relacion.CantidadMinima.Value,
                        BodegaPrincipal = bodega?.Nombre ?? "Bodega desconocida"
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

        // Usamos HashSet para evitar contar el mismo producto múltiples veces
        var productosConStockBajo = new HashSet<Guid>();

        foreach (var producto in productos)
        {
            // Obtener todas las relaciones ProductoBodega para este producto
            var relacionesBodegas = productoBodegas.Where(pb => pb.ProductoId == producto.Id).ToList();

            foreach (var relacion in relacionesBodegas)
            {
                // Si en cualquier bodega tiene stock bajo, contar el producto una vez
                if (relacion.CantidadMinima.HasValue && relacion.StockActual < relacion.CantidadMinima.Value)
                {
                    productosConStockBajo.Add(producto.Id);
                    break; // No necesitamos seguir verificando otras bodegas para este producto
                }
            }
        }

        return productosConStockBajo.Count;
    }

    private async Task<decimal> CalcularValorTotalInventarioAsync(CancellationToken ct)
    {
        var productos = await _unitOfWork.Products.ListAsync(filter: p => p.Activo, ct: ct);
        
        var valorTotal = 0m;

        foreach (var producto in productos)
        {
            var stockTotal = await _unitOfWork.Products.GetTotalStockAsync(producto.Id, ct);
            valorTotal += stockTotal * producto.CostoInicial;
        }

        return valorTotal;
    }
}
