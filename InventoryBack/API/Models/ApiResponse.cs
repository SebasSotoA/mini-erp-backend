using System.Text.Json.Serialization;

namespace InventoryBack.API.Models;

/// <summary>
/// Standard success response wrapper for API responses.
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful response for resource creation (201).
    /// </summary>
    public static ApiResponse<T> Created(T data, string message = "Recurso creado exitosamente.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 201,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful response with no content (204).
    /// </summary>
    public static ApiResponse<T> NoContent(string message = "Operación completada.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 204,
            Message = message,
            Data = default
        };
    }
}

/// <summary>
/// Standard error response wrapper for API errors.
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Creates a bad request error response (400).
    /// </summary>
    public static ApiErrorResponse BadRequest(string message, Dictionary<string, List<string>>? errors = null)
    {
        return new ApiErrorResponse
        {
            Success = false,
            StatusCode = 400,
            Message = message,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates a not found error response (404).
    /// </summary>
    public static ApiErrorResponse NotFound(string message)
    {
        return new ApiErrorResponse
        {
            Success = false,
            StatusCode = 404,
            Message = message
        };
    }

    /// <summary>
    /// Creates an internal server error response (500).
    /// </summary>
    public static ApiErrorResponse InternalServerError(string message = "Error interno del servidor.")
    {
        return new ApiErrorResponse
        {
            Success = false,
            StatusCode = 500,
            Message = message
        };
    }

    /// <summary>
    /// Creates an unauthorized error response (401).
    /// </summary>
    public static ApiErrorResponse Unauthorized(string message = "No autorizado.")
    {
        return new ApiErrorResponse
        {
            Success = false,
            StatusCode = 401,
            Message = message
        };
    }
}
