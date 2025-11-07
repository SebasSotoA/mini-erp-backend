using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class ProveedorRepository : EfGenericRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<bool> IdentificacionExistsAsync(string identificacion, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Proveedores
            .AsNoTracking()
            .Where(p => p.Identificacion == identificacion);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasPurchaseInvoicesAsync(Guid proveedorId, CancellationToken ct = default)
    {
        return await _db.FacturasCompra
            .AsNoTracking()
            .AnyAsync(fc => fc.ProveedorId == proveedorId, ct);
    }

    public async Task<(Proveedor? Proveedor, int PurchaseCount)> GetWithPurchaseCountAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _db.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (proveedor == null)
        {
            return (null, 0);
        }

        var purchaseCount = await _db.FacturasCompra
            .AsNoTracking()
            .CountAsync(fc => fc.ProveedorId == id, ct);

        return (proveedor, purchaseCount);
    }

    public async Task<Proveedor?> GetByIdentificacionAsync(string identificacion, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return null;

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Identificacion == identificacion, ct);
    }

    public async Task<(IEnumerable<Proveedor> Items, int TotalCount)> GetPagedAsync(
        ProveedorFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<Proveedor> query = _dbSet.AsNoTracking();

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

    private IQueryable<Proveedor> ApplyFilters(IQueryable<Proveedor> query, ProveedorFilterDto filters)
    {
        // Filter by nombre (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(nombre));
        }

        // Filter by identificacion (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Identificacion))
        {
            var identificacion = filters.Identificacion.Trim().ToLower();
            query = query.Where(p => p.Identificacion.ToLower().Contains(identificacion));
        }

        // Filter by correo (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Correo))
        {
            var correo = filters.Correo.Trim().ToLower();
            query = query.Where(p => p.Correo != null && p.Correo.ToLower().Contains(correo));
        }

        // Filter by status
        if (filters.Activo.HasValue)
        {
            query = query.Where(p => p.Activo == filters.Activo.Value);
        }

        return query;
    }

    private IQueryable<Proveedor> ApplyOrdering(
        IQueryable<Proveedor> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "identificacion" => orderDesc
                ? query.OrderByDescending(p => p.Identificacion)
                : query.OrderBy(p => p.Identificacion),

            "correo" => orderDesc
                ? query.OrderByDescending(p => p.Correo)
                : query.OrderBy(p => p.Correo),

            "fecha" => orderDesc
                ? query.OrderByDescending(p => p.FechaCreacion)
                : query.OrderBy(p => p.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(p => p.Nombre)
                : query.OrderBy(p => p.Nombre)
        };
    }
}
