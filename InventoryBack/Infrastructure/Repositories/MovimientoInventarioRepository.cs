using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

/// <summary>
/// MovimientoInventario repository implementation with specific queries.
/// READ-ONLY repository for audit and traceability.
/// </summary>
public class MovimientoInventarioRepository : EfGenericRepository<MovimientoInventario>, IMovimientoInventarioRepository
{
    public MovimientoInventarioRepository(InventoryDbContext db) : base(db)
    {
    }

    /// <summary>
    /// Override GetByIdAsync to include related entities for better performance.
    /// </summary>
    public new async Task<MovimientoInventario?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<(IEnumerable<MovimientoInventario> Items, int TotalCount)> GetPagedAsync(
        MovimientoInventarioFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<MovimientoInventario> query = _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)     // ?? Eager load Producto
            .Include(m => m.Bodega)       // ?? Eager load Bodega
            .Include(m => m.FacturaVenta) // ?? Eager load FacturaVenta
            .Include(m => m.FacturaCompra); // ?? Eager load FacturaCompra

        // Apply filters
        query = ApplyFilters(query, filters);

        // Apply ordering
        query = ApplyOrdering(query, filters.OrderBy, filters.OrderDesc);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByProductoIdAsync(
        Guid productoId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByBodegaIdAsync(
        Guid bodegaId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .Where(m => m.BodegaId == bodegaId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByFacturaVentaIdAsync(
        Guid facturaVentaId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .Where(m => m.FacturaVentaId == facturaVentaId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByFacturaCompraIdAsync(
        Guid facturaCompraId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .Where(m => m.FacturaCompraId == facturaCompraId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByFechaRangoAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Bodega)
            .Include(m => m.FacturaVenta)
            .Include(m => m.FacturaCompra)
            .Where(m => m.Fecha >= fechaDesde && m.Fecha <= fechaHasta)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private IQueryable<MovimientoInventario> ApplyFilters(
        IQueryable<MovimientoInventario> query,
        MovimientoInventarioFilterDto filters)
    {
        // Filter by product ID
        if (filters.ProductoId.HasValue)
        {
            query = query.Where(m => m.ProductoId == filters.ProductoId.Value);
        }

        // Filter by product name (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.ProductoNombre))
        {
            var searchTerm = filters.ProductoNombre.Trim().ToLower();
            query = query.Where(m => m.Producto != null && m.Producto.Nombre.ToLower().Contains(searchTerm));
        }

        // Filter by product SKU (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.ProductoSku))
        {
            var searchTerm = filters.ProductoSku.Trim().ToLower();
            query = query.Where(m => m.Producto != null && 
                                     m.Producto.CodigoSku != null && 
                                     m.Producto.CodigoSku.ToLower().Contains(searchTerm));
        }

        // Filter by warehouse ID
        if (filters.BodegaId.HasValue)
        {
            query = query.Where(m => m.BodegaId == filters.BodegaId.Value);
        }

        // Filter by warehouse name (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.BodegaNombre))
        {
            var searchTerm = filters.BodegaNombre.Trim().ToLower();
            query = query.Where(m => m.Bodega != null && m.Bodega.Nombre.ToLower().Contains(searchTerm));
        }

        // Filter by movement type
        if (!string.IsNullOrWhiteSpace(filters.TipoMovimiento))
        {
            var tipo = filters.TipoMovimiento.Trim().ToUpper();
            query = query.Where(m => m.TipoMovimiento.ToUpper() == tipo);
        }

        // Filter by invoice type (COMPRA or VENTA)
        if (!string.IsNullOrWhiteSpace(filters.TipoFactura))
        {
            var tipoUpper = filters.TipoFactura.Trim().ToUpper();
            if (tipoUpper == "COMPRA")
            {
                query = query.Where(m => m.FacturaCompraId != null);
            }
            else if (tipoUpper == "VENTA")
            {
                query = query.Where(m => m.FacturaVentaId != null);
            }
        }

        // Filter by date range
        if (filters.FechaDesde.HasValue)
        {
            query = query.Where(m => m.Fecha >= filters.FechaDesde.Value);
        }

        if (filters.FechaHasta.HasValue)
        {
            // Include entire day
            var fechaHastaFin = filters.FechaHasta.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(m => m.Fecha <= fechaHastaFin);
        }

        // Filter by quantity range
        if (filters.CantidadMinima.HasValue)
        {
            query = query.Where(m => m.Cantidad >= filters.CantidadMinima.Value);
        }

        if (filters.CantidadMaxima.HasValue)
        {
            query = query.Where(m => m.Cantidad <= filters.CantidadMaxima.Value);
        }

        // Filter by sales invoice
        if (filters.FacturaVentaId.HasValue)
        {
            query = query.Where(m => m.FacturaVentaId == filters.FacturaVentaId.Value);
        }

        // Filter by purchase invoice
        if (filters.FacturaCompraId.HasValue)
        {
            query = query.Where(m => m.FacturaCompraId == filters.FacturaCompraId.Value);
        }

        return query;
    }

    private IQueryable<MovimientoInventario> ApplyOrdering(
        IQueryable<MovimientoInventario> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "fecha";

        return field switch
        {
            "cantidad" => orderDesc
                ? query.OrderByDescending(m => m.Cantidad)
                : query.OrderBy(m => m.Cantidad),

            "tipomovimiento" => orderDesc
                ? query.OrderByDescending(m => m.TipoMovimiento)
                : query.OrderBy(m => m.TipoMovimiento),

            "productonombre" => orderDesc
                ? query.OrderByDescending(m => m.Producto != null ? m.Producto.Nombre : "")
                : query.OrderBy(m => m.Producto != null ? m.Producto.Nombre : ""),

            "bodeganombre" => orderDesc
                ? query.OrderByDescending(m => m.Bodega != null ? m.Bodega.Nombre : "")
                : query.OrderBy(m => m.Bodega != null ? m.Bodega.Nombre : ""),

            "fecha" or _ => orderDesc
                ? query.OrderByDescending(m => m.Fecha)
                : query.OrderBy(m => m.Fecha)
        };
    }
}
