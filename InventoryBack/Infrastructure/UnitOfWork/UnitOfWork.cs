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

    public UnitOfWork(
        InventoryDbContext db,
        IProductRepository productRepository,
        IGenericRepository<Categoria> categoryRepository,
        IGenericRepository<Bodega> warehouseRepository,
        IGenericRepository<ProductoBodega> productWarehouseRepository,
        IGenericRepository<CampoExtra> extraFieldRepository,
        IGenericRepository<ProductoCampoExtra> productExtraFieldRepository)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        Products = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        Categories = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        Warehouses = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
        ProductWarehouses = productWarehouseRepository ?? throw new ArgumentNullException(nameof(productWarehouseRepository));
        ExtraFields = extraFieldRepository ?? throw new ArgumentNullException(nameof(extraFieldRepository));
        ProductExtraFields = productExtraFieldRepository ?? throw new ArgumentNullException(nameof(productExtraFieldRepository));
    }

    public IProductRepository Products { get; }
    public IGenericRepository<Categoria> Categories { get; }
    public IGenericRepository<Bodega> Warehouses { get; }
    public IGenericRepository<ProductoBodega> ProductWarehouses { get; }
    public IGenericRepository<CampoExtra> ExtraFields { get; }
    public IGenericRepository<ProductoCampoExtra> ProductExtraFields { get; }

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
