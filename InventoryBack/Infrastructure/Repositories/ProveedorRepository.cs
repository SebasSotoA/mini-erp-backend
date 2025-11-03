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
}
