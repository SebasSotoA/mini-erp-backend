using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Repositories;

public class FacturaVentaRepository : EfGenericRepository<FacturaVenta>, IFacturaVentaRepository
{
    public FacturaVentaRepository(InventoryDbContext db) : base(db)
    {
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var prefix = $"FV-{now:yyyyMM}-";

        var lastInvoice = await _db.FacturasVenta
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

    public async Task<FacturaVenta?> GetByNumeroFacturaAsync(string numeroFactura, CancellationToken ct = default)
    {
        return await _db.FacturasVenta
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura, ct);
    }

    public async Task<FacturaVenta?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.FacturasVenta
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IEnumerable<FacturaVentaDetalle>> GetDetallesAsync(Guid facturaVentaId, CancellationToken ct = default)
    {
        return await _db.FacturasVentaDetalle
            .AsNoTracking()
            .Where(d => d.FacturaVentaId == facturaVentaId)
            .ToListAsync(ct);
    }

    public async Task<bool> CanDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _db.FacturasVenta
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (factura == null)
            return false;

        // Solo se puede eliminar si está en estado Pendiente
        return factura.Estado == "Pendiente";
    }
}
