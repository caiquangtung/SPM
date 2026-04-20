using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using notification_service.DTOs;
using notification_service.Extensions;
using notification_service.Services.Interfaces;

namespace notification_service.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        var items = await _notifications.GetForUserAsync(userId, unreadOnly, take, cancellationToken);
        return this.OkResponse(items, "Notifications retrieved successfully");
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        var stats = await _notifications.GetStatsAsync(userId, cancellationToken);
        return this.OkResponse(stats, "Unread count retrieved successfully");
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        var result = await _notifications.MarkReadAsync(userId, id, cancellationToken);

        if (result == null)
        {
            return this.NotFoundResponse("Notification not found", "NOTIFICATION_NOT_FOUND");
        }

        return this.OkResponse(result, "Notification marked as read");
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        var updated = await _notifications.MarkAllReadAsync(userId, cancellationToken);
        return this.OkResponse(new { updated }, "All notifications marked as read");
    }
}