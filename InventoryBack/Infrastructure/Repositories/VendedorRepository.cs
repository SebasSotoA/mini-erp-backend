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
}
