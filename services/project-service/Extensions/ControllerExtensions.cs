using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

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
}

