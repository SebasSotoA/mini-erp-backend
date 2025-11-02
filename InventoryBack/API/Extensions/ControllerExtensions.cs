using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Extensions;

/// <summary>
/// Extension methods for standardized API responses in controllers.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Returns a standardized 200 OK response with data.
    /// </summary>
    public static ActionResult<ApiResponse<T>> OkResponse<T>(
        this ControllerBase controller,
        T data,
        string message = "Operación exitosa.")
    {
        return controller.Ok(ApiResponse<T>.Ok(data, message));
    }

    /// <summary>
    /// Returns a standardized 201 Created response with data.
    /// </summary>
    public static ActionResult<ApiResponse<T>> CreatedResponse<T>(
        this ControllerBase controller,
        string actionName,
        object? routeValues,
        T data,
        string message = "Recurso creado exitosamente.")
    {
        var response = ApiResponse<T>.Created(data, message);
        return controller.CreatedAtAction(actionName, routeValues, response);
    }

    /// <summary>
    /// Returns a standardized 204 No Content response.
    /// </summary>
    public static ActionResult<ApiResponse<object>> NoContentResponse(
        this ControllerBase controller,
        string message = "Operación completada.")
    {
        return controller.Ok(ApiResponse<object>.NoContent(message));
    }
}
