using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class VendedorRepository : EfGenericRepository<Vendedor>, IVendedorRepository
{
    public VendedorRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<bool> IdentificacionExistsAsync(string identificacion, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Vendedores
            .AsNoTracking()
            .Where(v => v.Identificacion == identificacion);

        if (excludeId.HasValue)
        {
            query = query.Where(v => v.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasSalesInvoicesAsync(Guid vendedorId, CancellationToken ct = default)
    {
        return await _db.FacturasVenta
            .AsNoTracking()
            .AnyAsync(fv => fv.VendedorId == vendedorId, ct);
    }

    public async Task<(Vendedor? Vendedor, int SalesCount)> GetWithSalesCountAsync(Guid id, CancellationToken ct = default)
    {
        var vendedor = await _db.Vendedores
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (vendedor == null)
        {
            return (null, 0);
        }

        var salesCount = await _db.FacturasVenta
            .AsNoTracking()
            .CountAsync(fv => fv.VendedorId == id, ct);

        return (vendedor, salesCount);
    }

    public async Task<Vendedor?> GetByIdentificacionAsync(string identificacion, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return null;

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Identificacion == identificacion, ct);
    }

    public async Task<(IEnumerable<Vendedor> Items, int TotalCount)> GetPagedAsync(
        VendedorFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<Vendedor> query = _dbSet.AsNoTracking();

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

    private IQueryable<Vendedor> ApplyFilters(IQueryable<Vendedor> query, VendedorFilterDto filters)
    {
        // Filter by nombre (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(v => v.Nombre.ToLower().Contains(nombre));
        }

        // Filter by identificacion (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Identificacion))
        {
            var identificacion = filters.Identificacion.Trim().ToLower();
            query = query.Where(v => v.Identificacion.ToLower().Contains(identificacion));
        }

        // Filter by correo (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Correo))
        {
            var correo = filters.Correo.Trim().ToLower();
            query = query.Where(v => v.Correo != null && v.Correo.ToLower().Contains(correo));
        }

        // Filter by status
        if (filters.Activo.HasValue)
        {
            query = query.Where(v => v.Activo == filters.Activo.Value);
        }

        return query;
    }

    private IQueryable<Vendedor> ApplyOrdering(
        IQueryable<Vendedor> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "identificacion" => orderDesc
                ? query.OrderByDescending(v => v.Identificacion)
                : query.OrderBy(v => v.Identificacion),

            "correo" => orderDesc
                ? query.OrderByDescending(v => v.Correo)
                : query.OrderBy(v => v.Correo),

            "fecha" => orderDesc
                ? query.OrderByDescending(v => v.FechaCreacion)
                : query.OrderBy(v => v.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(v => v.Nombre)
                : query.OrderBy(v => v.Nombre)
        };
    }
}
