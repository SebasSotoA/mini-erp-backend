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
}
