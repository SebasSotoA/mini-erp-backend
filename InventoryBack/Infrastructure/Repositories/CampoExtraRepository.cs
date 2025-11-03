using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CampoExtra with specific queries.
/// </summary>
public class CampoExtraRepository : EfGenericRepository<CampoExtra>, ICampoExtraRepository
{
    public CampoExtraRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<CampoExtra?> GetByNameAsync(string nombre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentNullException(nameof(nombre));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(ce => ce.Nombre.ToLower() == nombre.ToLower(), ct);
    }

    public async Task<IEnumerable<CampoExtra>> GetActiveCamposAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ce => ce.Activo)
            .OrderBy(ce => ce.Nombre)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<CampoExtra> Items, int TotalCount)> GetPagedAsync(
        CampoExtraFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<CampoExtra> query = _dbSet.AsNoTracking();

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

    private IQueryable<CampoExtra> ApplyFilters(IQueryable<CampoExtra> query, CampoExtraFilterDto filters)
    {
        // Filter by nombre (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(ce => ce.Nombre.ToLower().Contains(nombre));
        }

        // Filter by tipoDato (exact match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.TipoDato))
        {
            var tipo = filters.TipoDato.Trim().ToLower();
            query = query.Where(ce => ce.TipoDato.ToLower() == tipo);
        }

        // Filter by esRequerido
        if (filters.EsRequerido.HasValue)
        {
            query = query.Where(ce => ce.EsRequerido == filters.EsRequerido.Value);
        }

        // Filter by status
        if (filters.Activo.HasValue)
        {
            query = query.Where(ce => ce.Activo == filters.Activo.Value);
        }

        return query;
    }

    private IQueryable<CampoExtra> ApplyOrdering(
        IQueryable<CampoExtra> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "tipodato" => orderDesc
                ? query.OrderByDescending(ce => ce.TipoDato)
                : query.OrderBy(ce => ce.TipoDato),

            "esrequerido" => orderDesc
                ? query.OrderByDescending(ce => ce.EsRequerido)
                : query.OrderBy(ce => ce.EsRequerido),

            "fecha" => orderDesc
                ? query.OrderByDescending(ce => ce.FechaCreacion)
                : query.OrderBy(ce => ce.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(ce => ce.Nombre)
                : query.OrderBy(ce => ce.Nombre)
        };
    }
}
