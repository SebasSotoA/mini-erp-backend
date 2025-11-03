using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class BodegaRepository : EfGenericRepository<Bodega>, IBodegaRepository
{
    public BodegaRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<Bodega?> GetByNameAsync(string nombre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentNullException(nameof(nombre));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Nombre.ToLower() == nombre.ToLower(), ct);
    }

    public async Task<IEnumerable<Bodega>> GetActiveBodegasAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.Activo)
            .OrderBy(b => b.Nombre)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<Bodega> Items, int TotalCount)> GetPagedAsync(
        BodegaFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<Bodega> query = _dbSet.AsNoTracking();

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

    public async Task<bool> HasProductsAsync(Guid bodegaId, CancellationToken ct = default)
    {
        return await _db.ProductoBodegas
            .AnyAsync(pb => pb.BodegaId == bodegaId, ct);
    }

    public async Task<bool> HasInvoicesAsync(Guid bodegaId, CancellationToken ct = default)
    {
        var hasFacturasVenta = await _db.FacturasVenta
            .AnyAsync(fv => fv.BodegaId == bodegaId, ct);

        if (hasFacturasVenta)
            return true;

        var hasFacturasCompra = await _db.FacturasCompra
            .AnyAsync(fc => fc.BodegaId == bodegaId, ct);

        return hasFacturasCompra;
    }

    public async Task<bool> HasMovementsAsync(Guid bodegaId, CancellationToken ct = default)
    {
        return await _db.MovimientosInventario
            .AnyAsync(mi => mi.BodegaId == bodegaId, ct);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private IQueryable<Bodega> ApplyFilters(IQueryable<Bodega> query, BodegaFilterDto filters)
    {
        // Filter by nombre (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(b => b.Nombre.ToLower().Contains(nombre));
        }

        // Filter by direccion (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Direccion))
        {
            var direccion = filters.Direccion.Trim().ToLower();
            query = query.Where(b => b.Direccion != null && b.Direccion.ToLower().Contains(direccion));
        }

        // Filter by status
        if (filters.Activo.HasValue)
        {
            query = query.Where(b => b.Activo == filters.Activo.Value);
        }

        return query;
    }

    private IQueryable<Bodega> ApplyOrdering(
        IQueryable<Bodega> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "direccion" => orderDesc
                ? query.OrderByDescending(b => b.Direccion)
                : query.OrderBy(b => b.Direccion),

            "fecha" => orderDesc
                ? query.OrderByDescending(b => b.FechaCreacion)
                : query.OrderBy(b => b.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(b => b.Nombre)
                : query.OrderBy(b => b.Nombre)
        };
    }
}
