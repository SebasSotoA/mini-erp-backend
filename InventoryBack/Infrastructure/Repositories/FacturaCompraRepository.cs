using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class FacturaCompraRepository : EfGenericRepository<FacturaCompra>, IFacturaCompraRepository
{
    public FacturaCompraRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var prefix = $"FC-{now:yyyyMM}-";

        var lastInvoice = await _db.FacturasCompra
            .Where(f => f.NumeroFactura.StartsWith(prefix))
            .OrderByDescending(f => f.NumeroFactura)
            .Select(f => f.NumeroFactura)
            .FirstOrDefaultAsync(ct);

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastInvoice.Substring(prefix.Length));
        var nextNumber = lastNumber + 1;

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<FacturaCompra?> GetByNumeroFacturaAsync(string numeroFactura, CancellationToken ct = default)
    {
        return await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura, ct);
    }

    public async Task<FacturaCompra?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IEnumerable<FacturaCompraDetalle>> GetDetallesAsync(Guid facturaCompraId, CancellationToken ct = default)
    {
        return await _db.FacturasCompraDetalle
            .AsNoTracking()
            .Where(d => d.FacturaCompraId == facturaCompraId)
            .ToListAsync(ct);
    }

    public async Task<bool> CanDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (factura == null)
            return false;

        // Solo se puede eliminar si está en estado Pendiente
        return factura.Estado == "Pendiente";
    }

    public async Task<(IEnumerable<FacturaCompra> Items, int TotalCount)> GetPagedAsync(
        FacturaCompraFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<FacturaCompra> query = _dbSet.AsNoTracking();

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

    // ========== PRIVATE HELPER METHODS ==========

    private IQueryable<FacturaCompra> ApplyFilters(IQueryable<FacturaCompra> query, FacturaCompraFilterDto filters)
    {
        // Filter by numero factura (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.NumeroFactura))
        {
            var numeroFactura = filters.NumeroFactura.Trim().ToLower();
            query = query.Where(f => f.NumeroFactura.ToLower().Contains(numeroFactura));
        }

        // Filter by proveedor ID
        if (filters.ProveedorId.HasValue)
        {
            query = query.Where(f => f.ProveedorId == filters.ProveedorId.Value);
        }

        // Filter by proveedor name (requires join)
        if (!string.IsNullOrWhiteSpace(filters.ProveedorNombre))
        {
            var proveedorNombre = filters.ProveedorNombre.Trim().ToLower();
            var proveedorIds = _db.Proveedores
                .Where(p => p.Nombre.ToLower().Contains(proveedorNombre))
                .Select(p => p.Id);
            
            query = query.Where(f => proveedorIds.Contains(f.ProveedorId));
        }

        // Filter by bodega ID
        if (filters.BodegaId.HasValue)
        {
            query = query.Where(f => f.BodegaId == filters.BodegaId.Value);
        }

        // Filter by bodega name (requires join)
        if (!string.IsNullOrWhiteSpace(filters.BodegaNombre))
        {
            var bodegaNombre = filters.BodegaNombre.Trim().ToLower();
            var bodegaIds = _db.Bodegas
                .Where(b => b.Nombre.ToLower().Contains(bodegaNombre))
                .Select(b => b.Id);
            
            query = query.Where(f => bodegaIds.Contains(f.BodegaId));
        }

        // Filter by estado
        if (!string.IsNullOrWhiteSpace(filters.Estado))
        {
            var estado = filters.Estado.Trim();
            query = query.Where(f => f.Estado == estado);
        }

        // Filter by fecha desde (inclusive)
        if (filters.FechaDesde.HasValue)
        {
            query = query.Where(f => f.Fecha.Date >= filters.FechaDesde.Value.Date);
        }

        // Filter by fecha hasta (inclusive)
        if (filters.FechaHasta.HasValue)
        {
            query = query.Where(f => f.Fecha.Date <= filters.FechaHasta.Value.Date);
        }

        return query;
    }

    private IQueryable<FacturaCompra> ApplyOrdering(
        IQueryable<FacturaCompra> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "fecha";

        return field switch
        {
            "numero" => orderDesc
                ? query.OrderByDescending(f => f.NumeroFactura)
                : query.OrderBy(f => f.NumeroFactura),

            "proveedor" => orderDesc
                ? query.OrderByDescending(f => f.ProveedorId)
                : query.OrderBy(f => f.ProveedorId),

            "bodega" => orderDesc
                ? query.OrderByDescending(f => f.BodegaId)
                : query.OrderBy(f => f.BodegaId),

            "estado" => orderDesc
                ? query.OrderByDescending(f => f.Estado)
                : query.OrderBy(f => f.Estado),

            "total" => orderDesc
                ? query.OrderByDescending(f => f.Total)
                : query.OrderBy(f => f.Total),

            "fecha" or _ => orderDesc
                ? query.OrderByDescending(f => f.Fecha)
                : query.OrderBy(f => f.Fecha)
        };
    }
}
