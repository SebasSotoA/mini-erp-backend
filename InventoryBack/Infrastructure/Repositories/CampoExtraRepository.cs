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
}
