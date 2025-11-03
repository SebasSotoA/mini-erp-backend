using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class FacturaVentaService : IFacturaVentaService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacturaVentaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<FacturaVentaDto> CreateAsync(CreateFacturaVentaDto dto, CancellationToken ct = default)
    {
        // ========== 1. VALIDATE BUSINESS RULES ==========

        // Validate: Bodega exists and is active
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(dto.BodegaId, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", dto.BodegaId);
        }

        if (!bodega.Activo)
        {
            throw new BusinessRuleException($"La bodega '{bodega.Nombre}' está inactiva.");
        }

        // Validate: Vendedor exists
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(dto.VendedorId, ct);
        if (vendedor == null)
        {
            throw new NotFoundException("Vendedor", dto.VendedorId);
        }

        // Validate: All products exist and have stock
        foreach (var item in dto.Items)
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(item.ProductoId, ct);
            if (producto == null)
            {
                throw new NotFoundException("Producto", item.ProductoId);
            }

            if (!producto.Activo)
            {
                throw new BusinessRuleException($"El producto '{producto.Nombre}' está inactivo.");
            }

            // Check stock in bodega
            var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(item.ProductoId, dto.BodegaId, ct);
            if (productoBodega == null)
            {
                throw new BusinessRuleException($"El producto '{producto.Nombre}' no está disponible en la bodega '{bodega.Nombre}'.");
            }

            if (productoBodega.CantidadInicial < item.Cantidad)
            {
                throw new BusinessRuleException($"Stock insuficiente para el producto '{producto.Nombre}'. Disponible: {productoBodega.CantidadInicial}, Solicitado: {item.Cantidad}");
            }
        }

        // ========== 2. GENERATE INVOICE NUMBER ==========

        var numeroFactura = await _unitOfWork.FacturasVenta.GetNextInvoiceNumberAsync(ct);

        // ========== 3. CREATE FACTURA ==========

        var factura = new FacturaVenta
        {
            Id = Guid.NewGuid(),
            NumeroFactura = numeroFactura,
            BodegaId = dto.BodegaId,
            VendedorId = dto.VendedorId,
            Fecha = dto.Fecha,
            FormaPago = dto.FormaPago,
            PlazoPago = dto.PlazoPago,
            MedioPago = dto.MedioPago,
            Observaciones = dto.Observaciones?.Trim(),
            Estado = "Completada", // ? Estado inicial
            Total = 0 // Se calculará
        };

        await _unitOfWork.FacturasVenta.AddAsync(factura, ct);

        // ========== 4. CREATE INVOICE DETAILS AND CALCULATE TOTAL ==========

        decimal total = 0;
        var detalles = new List<FacturaVentaDetalle>();

        foreach (var itemDto in dto.Items)
        {
            var totalLinea = CalculateTotalLinea(
                itemDto.PrecioUnitario,
                itemDto.Cantidad,
                itemDto.Descuento,
                itemDto.Impuesto
            );

            var detalle = new FacturaVentaDetalle
            {
                Id = Guid.NewGuid(),
                FacturaVentaId = factura.Id,
                ProductoId = itemDto.ProductoId,
                PrecioUnitario = itemDto.PrecioUnitario,
                Descuento = itemDto.Descuento,
                Impuesto = itemDto.Impuesto,
                Cantidad = itemDto.Cantidad,
                TotalLinea = totalLinea
            };

            detalles.Add(detalle);
            total += totalLinea;

            await _unitOfWork.FacturasVentaDetalle.AddAsync(detalle, ct);

            // Update stock
            var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(itemDto.ProductoId, dto.BodegaId, ct);
            if (productoBodega != null)
            {
                productoBodega.CantidadInicial -= itemDto.Cantidad;
                _unitOfWork.ProductoBodegas.Update(productoBodega);
            }

            // Create inventory movement
            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                ProductoId = itemDto.ProductoId,
                BodegaId = dto.BodegaId,
                Fecha = dto.Fecha,
                TipoMovimiento = "Venta",
                Cantidad = -itemDto.Cantidad, // Negativo porque es salida
                PrecioUnitario = itemDto.PrecioUnitario,
                Observacion = $"Venta - Factura {numeroFactura}",
                FacturaId = factura.Id
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento, ct);
        }

        factura.Total = total;
        _unitOfWork.FacturasVenta.Update(factura);

        // ========== 5. SAVE ALL CHANGES ==========

        await _unitOfWork.SaveChangesAsync(ct);

        // ========== 6. RETURN DTO ==========

        return await MapToDto(factura, detalles, bodega.Nombre, vendedor.Nombre, ct);
    }

    public async Task<FacturaVentaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _unitOfWork.FacturasVenta.GetByIdAsync(id, ct);
        if (factura == null)
        {
            return null;
        }

        var detalles = await _unitOfWork.FacturasVenta.GetDetallesAsync(id, ct);
        
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(factura.BodegaId, ct);
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(factura.VendedorId, ct);

        return await MapToDto(factura, detalles, bodega?.Nombre, vendedor?.Nombre, ct);
    }

    public async Task<IEnumerable<FacturaVentaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var facturas = await _unitOfWork.FacturasVenta.ListAsync(ct: ct);
        var result = new List<FacturaVentaDto>();

        foreach (var factura in facturas)
        {
            var detalles = await _unitOfWork.FacturasVenta.GetDetallesAsync(factura.Id, ct);
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(factura.BodegaId, ct);
            var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(factura.VendedorId, ct);

            result.Add(await MapToDto(factura, detalles, bodega?.Nombre, vendedor?.Nombre, ct));
        }

        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _unitOfWork.FacturasVenta.GetByIdAsync(id, ct);
        if (factura == null)
        {
            throw new NotFoundException("FacturaVenta", id);
        }

        // ========== SOFT DELETE: Solo se pueden anular facturas Completadas ==========

        if (factura.Estado == "Anulada")
        {
            throw new BusinessRuleException("La factura ya está anulada.");
        }

        if (factura.Estado != "Completada")
        {
            throw new BusinessRuleException(
                $"Solo se pueden anular facturas en estado 'Completada'. " +
                $"Esta factura está en estado '{factura.Estado}'.");
        }

        // Restore stock
        var detalles = await _unitOfWork.FacturasVenta.GetDetallesAsync(id, ct);
        foreach (var detalle in detalles)
        {
            var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(detalle.ProductoId, factura.BodegaId, ct);
            if (productoBodega != null)
            {
                productoBodega.CantidadInicial += detalle.Cantidad;
                _unitOfWork.ProductoBodegas.Update(productoBodega);
            }

            // Create reversal movement
            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                ProductoId = detalle.ProductoId,
                BodegaId = factura.BodegaId,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = "Anulación Venta",
                Cantidad = detalle.Cantidad, // Positivo porque devuelve stock
                PrecioUnitario = detalle.PrecioUnitario,
                Observacion = $"Anulación de venta - Factura {factura.NumeroFactura}",
                FacturaId = factura.Id
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento, ct);
        }

        // ========== SOFT DELETE: Cambiar estado a Anulada (NO eliminar físicamente) ==========
        factura.Estado = "Anulada";
        _unitOfWork.FacturasVenta.Update(factura);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private decimal CalculateTotalLinea(decimal precioUnitario, int cantidad, decimal? descuento, decimal? impuesto)
    {
        var subtotal = precioUnitario * cantidad;
        var totalConDescuento = subtotal - (descuento ?? 0);
        var totalFinal = totalConDescuento + (impuesto ?? 0);
        return totalFinal;
    }

    private async Task<FacturaVentaDto> MapToDto(
        FacturaVenta factura,
        IEnumerable<FacturaVentaDetalle> detalles,
        string? bodegaNombre,
        string? vendedorNombre,
        CancellationToken ct)
    {
        var detallesDto = new List<FacturaVentaDetalleDto>();

        foreach (var detalle in detalles)
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(detalle.ProductoId, ct);
            
            detallesDto.Add(new FacturaVentaDetalleDto
            {
                Id = detalle.Id,
                ProductoId = detalle.ProductoId,
                ProductoNombre = producto?.Nombre,
                ProductoSku = producto?.CodigoSku,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                Impuesto = detalle.Impuesto,
                TotalLinea = detalle.TotalLinea
            });
        }

        return new FacturaVentaDto
        {
            Id = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            BodegaId = factura.BodegaId,
            BodegaNombre = bodegaNombre,
            VendedorId = factura.VendedorId,
            VendedorNombre = vendedorNombre,
            Fecha = factura.Fecha,
            FormaPago = factura.FormaPago,
            PlazoPago = factura.PlazoPago,
            MedioPago = factura.MedioPago,
            Observaciones = factura.Observaciones,
            Estado = factura.Estado,
            Total = factura.Total,
            Items = detallesDto
        };
    }
}
