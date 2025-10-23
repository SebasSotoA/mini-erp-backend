using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

/// <summary>
/// ProductoBodega repository implementation with specific queries.
/// </summary>
public class ProductoBodegaRepository : EfGenericRepository<ProductoBodega>, IProductoBodegaRepository
{
    public ProductoBodegaRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<IEnumerable<ProductoBodega>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pb => pb.ProductoId == productId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProductoBodega>> GetByBodegaIdAsync(Guid bodegaId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pb => pb.BodegaId == bodegaId)
            .ToListAsync(ct);
    }

    public async Task<ProductoBodega?> GetByProductAndBodegaAsync(Guid productId, Guid bodegaId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(pb => pb.ProductoId == productId && pb.BodegaId == bodegaId, ct);
    }
}
