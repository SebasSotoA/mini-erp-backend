using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<Categoria?> GetByNameAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<Categoria>> GetActiveCategoriasAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets a paginated list of categorias with filtering and sorting.
    /// </summary>
    Task<(IEnumerable<Categoria> Items, int TotalCount)> GetPagedAsync(
        CategoriaFilterDto filters,
        CancellationToken ct = default);
    
    Task<bool> HasProductsAsync(Guid categoriaId, CancellationToken ct = default);
}
