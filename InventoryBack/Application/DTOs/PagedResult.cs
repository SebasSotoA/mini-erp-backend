using System.Text.Json.Serialization;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// Generic paginated result container.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = [];
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => Page > 1;
    
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => Page < TotalPages;
}
