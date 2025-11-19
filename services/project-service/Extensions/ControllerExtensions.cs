using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using project_service.DTOs;

namespace project_service.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <returns>User ID as Guid, or throws UnauthorizedAccessException if not found</returns>
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

    /// <summary>
    /// Gets the current user ID from JWT claims, returns null if not found
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <returns>User ID as Guid, or null if not found</returns>
    public static Guid? GetUserIdOrDefault(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

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

    /// <summary>
    /// Returns a created response (201) with location header
    /// </summary>
    public static IActionResult CreatedResponse<T>(this ControllerBase controller, string actionName, object routeValues, T data, string message = "Resource created successfully")
    {
        return controller.CreatedAtAction(actionName, routeValues, ApiResponse<T>.CreateSuccess(data, message));
    }
}

