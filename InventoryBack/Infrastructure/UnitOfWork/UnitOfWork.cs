using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;
using InventoryBack.Infrastructure.Data;
using InventoryBack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryBack.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation managing all repositories and database transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly InventoryDbContext _db;
    private bool _disposed;

    private readonly IProductRepository _products;
    private readonly ICategoriaRepository _categorias;
    private readonly IBodegaRepository _bodegas;
    private readonly ICampoExtraRepository _camposExtras;
    private readonly IProductoBodegaRepository _productoBodegas;
    private readonly IGenericRepository<ProductoCampoExtra> _productoCamposExtras;
    private readonly IFacturaVentaRepository _facturasVenta;
    private readonly IFacturaCompraRepository _facturasCompra;
    private readonly IGenericRepository<FacturaVentaDetalle> _facturasVentaDetalle;
    private readonly IGenericRepository<FacturaCompraDetalle> _facturasCompraDetalle;
    private readonly IVendedorRepository _vendedores;
    private readonly IProveedorRepository _proveedores;
    private readonly IMovimientoInventarioRepository _movimientosInventario;

    public UnitOfWork(InventoryDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _products = new ProductRepository(_db);
        _categorias = new CategoriaRepository(_db);
        _bodegas = new BodegaRepository(_db);
        _camposExtras = new CampoExtraRepository(_db);
        _productoBodegas = new ProductoBodegaRepository(_db);
        _productoCamposExtras = new EfGenericRepository<ProductoCampoExtra>(_db);
        _facturasVenta = new FacturaVentaRepository(_db);
        _facturasCompra = new FacturaCompraRepository(_db);
        _facturasVentaDetalle = new EfGenericRepository<FacturaVentaDetalle>(_db);
        _facturasCompraDetalle = new EfGenericRepository<FacturaCompraDetalle>(_db);
        _vendedores = new VendedorRepository(_db);
        _proveedores = new ProveedorRepository(_db);
        _movimientosInventario = new MovimientoInventarioRepository(_db);
    }

    public IProductRepository Products => _products;
    public ICategoriaRepository Categorias => _categorias;
    public IBodegaRepository Bodegas => _bodegas;
    public ICampoExtraRepository CamposExtras => _camposExtras;
    public IProductoBodegaRepository ProductoBodegas => _productoBodegas;
    public IGenericRepository<ProductoCampoExtra> ProductoCamposExtras => _productoCamposExtras;
    public IFacturaVentaRepository FacturasVenta => _facturasVenta;
    public IFacturaCompraRepository FacturasCompra => _facturasCompra;
    public IGenericRepository<FacturaVentaDetalle> FacturasVentaDetalle => _facturasVentaDetalle;
    public IGenericRepository<FacturaCompraDetalle> FacturasCompraDetalle => _facturasCompraDetalle;
    public IVendedorRepository Vendedores => _vendedores;
    public IProveedorRepository Proveedores => _proveedores;
    public IMovimientoInventarioRepository MovimientosInventario => _movimientosInventario;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await _db.Database.BeginTransactionAsync(ct);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _db.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
