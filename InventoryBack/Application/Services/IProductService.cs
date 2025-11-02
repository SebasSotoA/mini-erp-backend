using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Services;

/// <summary>
/// Product service interface for business logic operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product. Supports both quick and advanced creation flows.
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product</returns>
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product or null if not found</returns>
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a paginated list of products with advanced filtering.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated result of products</returns>
    Task<PagedResult<ProductDto>> GetPagedAsync(
        ProductFilterDto filters,
        CancellationToken ct = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Update data</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);

    /// <summary>
    /// Activates a product (sets Activo to true).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a product (sets Activo to false).
    /// Alias for soft delete operation.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    Task DeactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes a product from the database.
    /// Only allowed if the product is not referenced in any invoices.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default);

    // ========== WAREHOUSE MANAGEMENT ==========

    /// <summary>
    /// Adds a product to an additional warehouse.
    /// </summary>
    Task AddToWarehouseAsync(Guid productId, AddProductoBodegaDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates product quantities/thresholds in a specific warehouse.
    /// </summary>
    Task UpdateWarehouseQuantitiesAsync(Guid productId, Guid bodegaId, UpdateProductoBodegaDto dto, CancellationToken ct = default);

    /// <summary>
    /// Removes a product from a warehouse.
    /// </summary>
    Task RemoveFromWarehouseAsync(Guid productId, Guid bodegaId, CancellationToken ct = default);

    /// <summary>
    /// Gets all warehouses where a product is stored.
    /// </summary>
    Task<IEnumerable<ProductoBodegaDetailDto>> GetProductWarehousesAsync(Guid productId, CancellationToken ct = default);

    // ========== EXTRA FIELDS MANAGEMENT ==========

    /// <summary>
    /// Sets or updates an extra field value for a product.
    /// </summary>
    Task SetExtraFieldAsync(Guid productId, Guid campoExtraId, string valor, CancellationToken ct = default);

    /// <summary>
    /// Removes an extra field from a product.
    /// </summary>
    Task RemoveExtraFieldAsync(Guid productId, Guid campoExtraId, CancellationToken ct = default);

    /// <summary>
    /// Gets all extra fields assigned to a product.
    /// </summary>
    Task<IEnumerable<ProductoCampoExtraDetailDto>> GetProductExtraFieldsAsync(Guid productId, CancellationToken ct = default);
}
