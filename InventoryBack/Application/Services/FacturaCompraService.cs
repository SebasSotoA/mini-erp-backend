using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class FacturaCompraService : IFacturaCompraService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacturaCompraService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<FacturaCompraDto> CreateAsync(CreateFacturaCompraDto dto, CancellationToken ct = default)
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

        // Validate: Proveedor exists and is active
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.ProveedorId, ct);
        if (proveedor == null)
        {
            throw new NotFoundException("Proveedor", dto.ProveedorId);
        }

        if (!proveedor.Activo)
        {
            throw new BusinessRuleException($"El proveedor '{proveedor.Nombre}' está inactivo.");
        }

        // Validate: All products exist and are active
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
        }

        // ========== 2. GENERATE INVOICE NUMBER ==========

        var numeroFactura = await _unitOfWork.FacturasCompra.GetNextInvoiceNumberAsync(ct);

        // ========== 3. CREATE FACTURA ==========

        var factura = new FacturaCompra
        {
            Id = Guid.NewGuid(),
            NumeroFactura = numeroFactura,
            BodegaId = dto.BodegaId,
            ProveedorId = dto.ProveedorId,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones?.Trim(),
            Estado = "Completada",
            Total = 0 // Se calculará
        };

        await _unitOfWork.FacturasCompra.AddAsync(factura, ct);

        // ========== 4. CREATE INVOICE DETAILS AND CALCULATE TOTAL ==========

        decimal total = 0;
        var detalles = new List<FacturaCompraDetalle>();

        foreach (var itemDto in dto.Items)
        {
            var totalLinea = CalculateTotalLinea(
                itemDto.CostoUnitario,
                itemDto.Cantidad,
                itemDto.Descuento,
                itemDto.Impuesto
            );

            var detalle = new FacturaCompraDetalle
            {
                Id = Guid.NewGuid(),
                FacturaCompraId = factura.Id,
                ProductoId = itemDto.ProductoId,
                CostoUnitario = itemDto.CostoUnitario,
                Descuento = itemDto.Descuento,
                Impuesto = itemDto.Impuesto,
                Cantidad = itemDto.Cantidad,
                TotalLinea = totalLinea
            };

            detalles.Add(detalle);
            total += totalLinea;

            await _unitOfWork.FacturasCompraDetalle.AddAsync(detalle, ct);

            // ========== ACTUALIZAR STOCK (CRÍTICO) ==========
            var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(itemDto.ProductoId, dto.BodegaId, ct);
            if (productoBodega != null)
            {
                productoBodega.StockActual += itemDto.Cantidad;
                _unitOfWork.ProductoBodegas.Update(productoBodega);
            }
            else
            {
                // If product doesn't exist in this bodega, create relationship
                var newProductoBodega = new ProductoBodega
                {
                    Id = Guid.NewGuid(),
                    ProductoId = itemDto.ProductoId,
                    BodegaId = dto.BodegaId,
                    StockActual = itemDto.Cantidad,
                    CantidadMinima = null,
                    CantidadMaxima = null
                };
                await _unitOfWork.ProductoBodegas.AddAsync(newProductoBodega, ct);
            }

            // ========== CREATE INVENTORY MOVEMENT (TRAZABILIDAD) ==========
            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                ProductoId = itemDto.ProductoId,
                BodegaId = dto.BodegaId,
                Fecha = dto.Fecha,
                TipoMovimiento = "ENTRADA",
                Cantidad = itemDto.Cantidad, // Positivo, el tipo indica si es entrada/salida
                CostoUnitario = itemDto.CostoUnitario,
                Observacion = $"Compra - Factura {numeroFactura}",
                FacturaVentaId = null,
                FacturaCompraId = factura.Id
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento, ct);
        }

        factura.Total = total;
        _unitOfWork.FacturasCompra.Update(factura);

        // ========== 5. SAVE ALL CHANGES ==========

        await _unitOfWork.SaveChangesAsync(ct);

        // ========== 6. RETURN DTO ==========

        return await MapToDto(factura, detalles, bodega.Nombre, proveedor.Nombre, ct);
    }

    public async Task<FacturaCompraDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _unitOfWork.FacturasCompra.GetByIdAsync(id, ct);
        if (factura == null)
        {
            return null;
        }

        var detalles = await _unitOfWork.FacturasCompra.GetDetallesAsync(id, ct);
        
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(factura.BodegaId, ct);
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(factura.ProveedorId, ct);

        return await MapToDto(factura, detalles, bodega?.Nombre, proveedor?.Nombre, ct);
    }

    public async Task<IEnumerable<FacturaCompraDto>> GetAllAsync(CancellationToken ct = default)
    {
        var facturas = await _unitOfWork.FacturasCompra.ListAsync(ct: ct);
        var result = new List<FacturaCompraDto>();

        foreach (var factura in facturas)
        {
            var detalles = await _unitOfWork.FacturasCompra.GetDetallesAsync(factura.Id, ct);
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(factura.BodegaId, ct);
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(factura.ProveedorId, ct);

            result.Add(await MapToDto(factura, detalles, bodega?.Nombre, proveedor?.Nombre, ct));
        }

        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _unitOfWork.FacturasCompra.GetByIdAsync(id, ct);
        if (factura == null)
        {
            throw new NotFoundException("FacturaCompra", id);
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

        // ========== RESTORE STOCK (CRÍTICO) ==========
        var detalles = await _unitOfWork.FacturasCompra.GetDetallesAsync(id, ct);
        foreach (var detalle in detalles)
        {
            var productoBodega = await _unitOfWork.ProductoBodegas.GetByProductAndBodegaAsync(detalle.ProductoId, factura.BodegaId, ct);
            if (productoBodega != null)
            {
                productoBodega.StockActual -= detalle.Cantidad;
                _unitOfWork.ProductoBodegas.Update(productoBodega);
            }

            // Create reversal movement
            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                ProductoId = detalle.ProductoId,
                BodegaId = factura.BodegaId,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = "AJUSTE_NEGATIVO",
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Observacion = $"Anulación de compra - Factura {factura.NumeroFactura}",
                FacturaVentaId = null,
                FacturaCompraId = factura.Id
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento, ct);
        }

        // ========== SOFT DELETE: Cambiar estado a Anulada (NO eliminar físicamente) ==========
        factura.Estado = "Anulada";
        _unitOfWork.FacturasCompra.Update(factura);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private decimal CalculateTotalLinea(decimal costoUnitario, int cantidad, decimal? descuento, decimal? impuesto)
    {
        var subtotal = costoUnitario * cantidad;
        var totalConDescuento = subtotal - (descuento ?? 0);
        var totalFinal = totalConDescuento + (impuesto ?? 0);
        return totalFinal;
    }

    private async Task<FacturaCompraDto> MapToDto(
        FacturaCompra factura,
        IEnumerable<FacturaCompraDetalle> detalles,
        string? bodegaNombre,
        string? proveedorNombre,
        CancellationToken ct)
    {
        var detallesDto = new List<FacturaCompraDetalleDto>();

        foreach (var detalle in detalles)
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(detalle.ProductoId, ct);
            
            detallesDto.Add(new FacturaCompraDetalleDto
            {
                Id = detalle.Id,
                ProductoId = detalle.ProductoId,
                ProductoNombre = producto?.Nombre,
                ProductoSku = producto?.CodigoSku,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Descuento = detalle.Descuento,
                Impuesto = detalle.Impuesto,
                TotalLinea = detalle.TotalLinea
            });
        }

        return new FacturaCompraDto
        {
            Id = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            BodegaId = factura.BodegaId,
            BodegaNombre = bodegaNombre,
            ProveedorId = factura.ProveedorId,
            ProveedorNombre = proveedorNombre,
            Fecha = factura.Fecha,
            Observaciones = factura.Observaciones,
            Estado = factura.Estado,
            Total = factura.Total,
            Items = detallesDto
        };
    }
}
