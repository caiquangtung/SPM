using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using notification_service.DTOs;

namespace notification_service.Extensions;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token claims.");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format in token claims.");
        }

        return userId;
    }

    public static IActionResult OkResponse<T>(this ControllerBase controller, T data, string message = "Operation successful")
    {
        return controller.Ok(ApiResponse<T>.CreateSuccess(data, message));
    }

    public static IActionResult OkResponse(this ControllerBase controller, string message = "Operation successful")
    {
        return controller.Ok(ApiResponse.CreateSuccess(message));
    }

    public static IActionResult BadRequestResponse(this ControllerBase controller, string message, string? errorCode = null)
    {
        return controller.BadRequest(ApiResponse.CreateFail(message, errorCode));
    }

    public static IActionResult UnauthorizedResponse(this ControllerBase controller, string message, string? errorCode = null)
    {
        return controller.Unauthorized(ApiResponse.CreateFail(message, errorCode));
    }

    public static IActionResult NotFoundResponse(this ControllerBase controller, string message = "Resource not found", string? errorCode = null)
    {
        return controller.NotFound(ApiResponse.CreateFail(message, errorCode));
    }
}