using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

public interface ICampoExtraService
{
    Task<CampoExtraDto> CreateAsync(CreateCampoExtraDto dto, CancellationToken ct = default);
    Task<CampoExtraDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CampoExtraDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default);
    Task<PagedResult<CampoExtraDto>> GetPagedAsync(CampoExtraFilterDto filters, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateCampoExtraDto dto, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all products that have this extra field assigned with advanced filtering and pagination.
    /// Uses the same filters as the Products endpoint.
    /// </summary>
    /// <param name="campoExtraId">Extra field ID</param>
    /// <param name="filters">Product filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of products with this extra field and their values</returns>
    Task<PagedResult<ProductInCampoExtraDto>> GetProductsInCampoExtraAsync(
        Guid campoExtraId, 
        ProductFilterDto filters, 
        CancellationToken ct = default);
}
