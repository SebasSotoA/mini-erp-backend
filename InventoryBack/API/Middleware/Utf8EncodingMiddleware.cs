using System.Text;

namespace InventoryBack.API.Middleware;

/// <summary>
/// Middleware to ensure all responses have UTF-8 encoding in Content-Type header.
/// This prevents encoding issues with special characters (tildes, eñes, etc.) in production.
/// </summary>
public class Utf8EncodingMiddleware
{
    private readonly RequestDelegate _next;

    public Utf8EncodingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Set UTF-8 encoding for response before processing
        context.Response.OnStarting(() =>
        {
            // Only modify Content-Type if it's not already set or doesn't have charset
            if (context.Response.ContentType != null && 
                !context.Response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
            {
                // Add charset=utf-8 to existing Content-Type
                context.Response.ContentType = $"{context.Response.ContentType}; charset=utf-8";
            }
            else if (context.Response.ContentType == null)
            {
                // Set default Content-Type with UTF-8 for JSON responses
                context.Response.ContentType = "application/json; charset=utf-8";
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
