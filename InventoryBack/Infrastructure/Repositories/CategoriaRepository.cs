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

    public async Task<bool> HasProductsAsync(Guid categoriaId, CancellationToken ct = default)
    {
        return await _db.Productos
            .AnyAsync(p => p.CategoriaId == categoriaId, ct);
    }
}
