using Microsoft.AspNetCore.Mvc;
using user_service.DTOs;

namespace user_service.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    /// Returns a successful API response with data
    /// </summary>
    public static IActionResult OkResponse<T>(this ControllerBase controller, T data, string message = "Operation successful")
    {
        return controller.Ok(ApiResponse<T>.CreateSuccess(data, message));
    }

    /// <summary>
    /// Returns a successful API response with message only
    /// </summary>
    public static IActionResult OkResponse(this ControllerBase controller, string message = "Operation successful")
    {
        return controller.Ok(ApiResponse.CreateSuccess(message));
    }

    /// <summary>
    /// Returns a bad request response
    /// </summary>
    public static IActionResult BadRequestResponse(this ControllerBase controller, string message, string? errorCode = null)
    {
        return controller.BadRequest(ApiResponse.CreateFail(message, errorCode));
    }

    /// <summary>
    /// Returns an unauthorized response
    /// </summary>
    public static IActionResult UnauthorizedResponse(this ControllerBase controller, string message, string? errorCode = null)
    {
        return controller.Unauthorized(ApiResponse.CreateFail(message, errorCode));
    }

    /// <summary>
    /// Returns a not found response
    /// </summary>
    public static IActionResult NotFoundResponse(this ControllerBase controller, string message = "Resource not found", string? errorCode = null)
    {
        return controller.NotFound(ApiResponse.CreateFail(message, errorCode));
    }
}

