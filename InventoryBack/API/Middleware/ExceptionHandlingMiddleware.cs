using System.Net;
using System.Text.Json;
using InventoryBack.Application.Exceptions;
using InventoryBack.API.Models;

namespace InventoryBack.API.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// Converts domain exceptions into appropriate HTTP responses with standardized format.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ApiErrorResponse errorResponse;

        switch (exception)
        {
            case BusinessRuleException businessEx:
                _logger.LogWarning(businessEx, "Business rule violation: {Message}", businessEx.Message);
                errorResponse = ApiErrorResponse.BadRequest(businessEx.Message);
                break;

            case NotFoundException notFoundEx:
                _logger.LogWarning(notFoundEx, "Resource not found: {Message}", notFoundEx.Message);
                errorResponse = ApiErrorResponse.NotFound(notFoundEx.Message);
                break;

            case FluentValidation.ValidationException validationEx:
                _logger.LogWarning(validationEx, "Validation error: {Message}", validationEx.Message);
                errorResponse = ApiErrorResponse.BadRequest(
                    "Error de validación.",
                    FormatValidationErrors(validationEx)
                );
                break;

            case UnauthorizedAccessException:
                _logger.LogWarning(exception, "Unauthorized access attempt");
                errorResponse = ApiErrorResponse.Unauthorized("No tiene permisos para realizar esta operación.");
                break;

            default:
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                errorResponse = ApiErrorResponse.InternalServerError(
                    "Ha ocurrido un error interno en el servidor."
                );
                break;
        }

        // Set path
        errorResponse.Path = context.Request.Path.Value;

        // Prepare response
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, jsonOptions)
        );
    }

    private static Dictionary<string, List<string>> FormatValidationErrors(
        FluentValidation.ValidationException validationException)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var error in validationException.Errors)
        {
            var propertyName = ToCamelCase(error.PropertyName);
            
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }

            errors[propertyName].Add(error.ErrorMessage);
        }

        return errors;
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLower(str[0]) + str.Substring(1);
    }
}
