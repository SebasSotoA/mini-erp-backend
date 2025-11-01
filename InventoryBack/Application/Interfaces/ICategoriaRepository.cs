using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Interfaces;

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<Categoria?> GetByNameAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<Categoria>> GetActiveCategoriasAsync(CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid categoriaId, CancellationToken ct = default);
}
