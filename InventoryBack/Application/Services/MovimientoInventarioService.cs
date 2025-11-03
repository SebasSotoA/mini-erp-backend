using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service implementation for MovimientoInventario (READ-ONLY).
/// Provides audit and traceability for inventory movements.
/// </summary>
public class MovimientoInventarioService : IMovimientoInventarioService
{
    private readonly IUnitOfWork _unitOfWork;

    public MovimientoInventarioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PagedResult<MovimientoInventarioDto>> GetPagedAsync(
        MovimientoInventarioFilterDto filters,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.MovimientosInventario.GetPagedAsync(filters, ct);

        var dtos = new List<MovimientoInventarioDto>();

        foreach (var movimiento in items)
        {
            dtos.Add(await MapToDtoAsync(movimiento, ct));
        }

        return new PagedResult<MovimientoInventarioDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovimientoInventarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var movimiento = await _unitOfWork.MovimientosInventario.GetByIdAsync(id, ct);
        
        if (movimiento == null)
        {
            return null;
        }

        return await MapToDtoAsync(movimiento, ct);
    }

    public async Task<IEnumerable<MovimientoInventarioDto>> GetByProductoIdAsync(
        Guid productoId,
        CancellationToken ct = default)
    {
        var movimientos = await _unitOfWork.MovimientosInventario.GetByProductoIdAsync(productoId, ct);
        
        var dtos = new List<MovimientoInventarioDto>();
        foreach (var movimiento in movimientos)
        {
            dtos.Add(await MapToDtoAsync(movimiento, ct));
        }

        return dtos;
    }

    public async Task<IEnumerable<MovimientoInventarioDto>> GetByBodegaIdAsync(
        Guid bodegaId,
        CancellationToken ct = default)
    {
        var movimientos = await _unitOfWork.MovimientosInventario.GetByBodegaIdAsync(bodegaId, ct);
        
        var dtos = new List<MovimientoInventarioDto>();
        foreach (var movimiento in movimientos)
        {
            dtos.Add(await MapToDtoAsync(movimiento, ct));
        }

        return dtos;
    }

    public async Task<IEnumerable<MovimientoInventarioDto>> GetByFacturaVentaIdAsync(
        Guid facturaVentaId,
        CancellationToken ct = default)
    {
        var movimientos = await _unitOfWork.MovimientosInventario.GetByFacturaVentaIdAsync(facturaVentaId, ct);
        
        var dtos = new List<MovimientoInventarioDto>();
        foreach (var movimiento in movimientos)
        {
            dtos.Add(await MapToDtoAsync(movimiento, ct));
        }

        return dtos;
    }

    public async Task<IEnumerable<MovimientoInventarioDto>> GetByFacturaCompraIdAsync(
        Guid facturaCompraId,
        CancellationToken ct = default)
    {
        var movimientos = await _unitOfWork.MovimientosInventario.GetByFacturaCompraIdAsync(facturaCompraId, ct);
        
        var dtos = new List<MovimientoInventarioDto>();
        foreach (var movimiento in movimientos)
        {
            dtos.Add(await MapToDtoAsync(movimiento, ct));
        }

        return dtos;
    }

    // ========== PRIVATE HELPER METHODS ==========

    private async Task<MovimientoInventarioDto> MapToDtoAsync(
        Domain.Entities.MovimientoInventario movimiento,
        CancellationToken ct)
    {
        var dto = new MovimientoInventarioDto
        {
            Id = movimiento.Id,
            ProductoId = movimiento.ProductoId,
            BodegaId = movimiento.BodegaId,
            Fecha = movimiento.Fecha,
            TipoMovimiento = movimiento.TipoMovimiento,
            Cantidad = movimiento.Cantidad,
            CostoUnitario = movimiento.CostoUnitario,
            PrecioUnitario = movimiento.PrecioUnitario,
            Observacion = movimiento.Observacion,
            FacturaVentaId = movimiento.FacturaVentaId,
            FacturaCompraId = movimiento.FacturaCompraId
        };

        // Get product details (use navigation property if loaded, otherwise query)
        if (movimiento.Producto != null)
        {
            dto.ProductoNombre = movimiento.Producto.Nombre;
            dto.ProductoSku = movimiento.Producto.CodigoSku;
        }
        else
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(movimiento.ProductoId, ct);
            if (producto != null)
            {
                dto.ProductoNombre = producto.Nombre;
                dto.ProductoSku = producto.CodigoSku;
            }
        }

        // Get warehouse details (use navigation property if loaded, otherwise query)
        if (movimiento.Bodega != null)
        {
            dto.BodegaNombre = movimiento.Bodega.Nombre;
        }
        else
        {
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(movimiento.BodegaId, ct);
            if (bodega != null)
            {
                dto.BodegaNombre = bodega.Nombre;
            }
        }

        // Get sales invoice number (use navigation property if loaded, otherwise query)
        if (movimiento.FacturaVentaId.HasValue)
        {
            if (movimiento.FacturaVenta != null)
            {
                dto.FacturaVentaNumero = movimiento.FacturaVenta.NumeroFactura;
            }
            else
            {
                var facturaVenta = await _unitOfWork.FacturasVenta.GetByIdAsync(movimiento.FacturaVentaId.Value, ct);
                if (facturaVenta != null)
                {
                    dto.FacturaVentaNumero = facturaVenta.NumeroFactura;
                }
            }
        }

        // Get purchase invoice number (use navigation property if loaded, otherwise query)
        if (movimiento.FacturaCompraId.HasValue)
        {
            if (movimiento.FacturaCompra != null)
            {
                dto.FacturaCompraNumero = movimiento.FacturaCompra.NumeroFactura;
            }
            else
            {
                var facturaCompra = await _unitOfWork.FacturasCompra.GetByIdAsync(movimiento.FacturaCompraId.Value, ct);
                if (facturaCompra != null)
                {
                    dto.FacturaCompraNumero = facturaCompra.NumeroFactura;
                }
            }
        }

        return dto;
    }
}
