using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class FacturaCompraRepository : EfGenericRepository<FacturaCompra>, IFacturaCompraRepository
{
    public FacturaCompraRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var prefix = $"FC-{now:yyyyMM}-";

        var lastInvoice = await _db.FacturasCompra
            .Where(f => f.NumeroFactura.StartsWith(prefix))
            .OrderByDescending(f => f.NumeroFactura)
            .Select(f => f.NumeroFactura)
            .FirstOrDefaultAsync(ct);

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastInvoice.Substring(prefix.Length));
        var nextNumber = lastNumber + 1;

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<FacturaCompra?> GetByNumeroFacturaAsync(string numeroFactura, CancellationToken ct = default)
    {
        return await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura, ct);
    }

    public async Task<FacturaCompra?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IEnumerable<FacturaCompraDetalle>> GetDetallesAsync(Guid facturaCompraId, CancellationToken ct = default)
    {
        return await _db.FacturasCompraDetalle
            .AsNoTracking()
            .Where(d => d.FacturaCompraId == facturaCompraId)
            .ToListAsync(ct);
    }

    public async Task<bool> CanDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _db.FacturasCompra
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (factura == null)
            return false;

        // Solo se puede eliminar si está en estado Pendiente
        return factura.Estado == "Pendiente";
    }
}
