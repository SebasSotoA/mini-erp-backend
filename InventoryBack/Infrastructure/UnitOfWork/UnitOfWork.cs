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
        IProductoBodegaRepository productoBodegaRepository,
        IGenericRepository<Categoria> categoryRepository,
        IBodegaRepository bodegaRepository,
        ICampoExtraRepository campoExtraRepository,
        IGenericRepository<ProductoCampoExtra> productoCampoExtraRepository)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        Products = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        ProductoBodegas = productoBodegaRepository ?? throw new ArgumentNullException(nameof(productoBodegaRepository));
        Categories = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        Bodegas = bodegaRepository ?? throw new ArgumentNullException(nameof(bodegaRepository));
        CamposExtras = campoExtraRepository ?? throw new ArgumentNullException(nameof(campoExtraRepository));
        ProductoCamposExtras = productoCampoExtraRepository ?? throw new ArgumentNullException(nameof(productoCampoExtraRepository));
    }

    public IProductRepository Products { get; }
    public IProductoBodegaRepository ProductoBodegas { get; }
    public IGenericRepository<Categoria> Categories { get; }
    public IBodegaRepository Bodegas { get; }
    public ICampoExtraRepository CamposExtras { get; }
    public IGenericRepository<ProductoCampoExtra> ProductoCamposExtras { get; }

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
