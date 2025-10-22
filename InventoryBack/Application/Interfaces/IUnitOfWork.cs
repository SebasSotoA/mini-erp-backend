using InventoryBack.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryBack.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions and repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Product repository
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Category repository
    /// </summary>
    IGenericRepository<Categoria> Categories { get; }

    /// <summary>
    /// Warehouse (Bodega) repository
    /// </summary>
    IGenericRepository<Bodega> Warehouses { get; }

    /// <summary>
    /// Product-Warehouse repository
    /// </summary>
    IGenericRepository<ProductoBodega> ProductWarehouses { get; }

    /// <summary>
    /// Extra field repository
    /// </summary>
    IGenericRepository<CampoExtra> ExtraFields { get; }

    /// <summary>
    /// Product-Extra field repository
    /// </summary>
    IGenericRepository<ProductoCampoExtra> ProductExtraFields { get; }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of affected rows</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Database transaction</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
