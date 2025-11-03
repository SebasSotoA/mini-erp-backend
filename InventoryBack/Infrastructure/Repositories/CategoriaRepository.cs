using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class CategoriaRepository : EfGenericRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<Categoria?> GetByNameAsync(string nombre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentNullException(nameof(nombre));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower(), ct);
    }

    public async Task<IEnumerable<Categoria>> GetActiveCategoriasAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<Categoria> Items, int TotalCount)> GetPagedAsync(
        CategoriaFilterDto filters,
        CancellationToken ct = default)
    {
        // Validate and normalize pagination
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

        IQueryable<Categoria> query = _dbSet.AsNoTracking();

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

    public async Task<bool> HasProductsAsync(Guid categoriaId, CancellationToken ct = default)
    {
        return await _db.Productos
            .AnyAsync(p => p.CategoriaId == categoriaId, ct);
    }

    // ========== PRIVATE HELPER METHODS ==========

    private IQueryable<Categoria> ApplyFilters(IQueryable<Categoria> query, CategoriaFilterDto filters)
    {
        // Filter by nombre (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            var nombre = filters.Nombre.Trim().ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(nombre));
        }

        // Filter by descripcion (partial match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(filters.Descripcion))
        {
            var descripcion = filters.Descripcion.Trim().ToLower();
            query = query.Where(c => c.Descripcion != null && c.Descripcion.ToLower().Contains(descripcion));
        }

        // Filter by status
        if (filters.Activo.HasValue)
        {
            query = query.Where(c => c.Activo == filters.Activo.Value);
        }

        return query;
    }

    private IQueryable<Categoria> ApplyOrdering(
        IQueryable<Categoria> query,
        string? orderBy,
        bool orderDesc)
    {
        var field = orderBy?.ToLower() ?? "nombre";

        return field switch
        {
            "descripcion" => orderDesc
                ? query.OrderByDescending(c => c.Descripcion)
                : query.OrderBy(c => c.Descripcion),

            "fecha" => orderDesc
                ? query.OrderByDescending(c => c.FechaCreacion)
                : query.OrderBy(c => c.FechaCreacion),

            "nombre" or _ => orderDesc
                ? query.OrderByDescending(c => c.Nombre)
                : query.OrderBy(c => c.Nombre)
        };
    }
}
