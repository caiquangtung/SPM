using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using notification_service.Data;
using notification_service.DTOs;
using notification_service.Hubs;
using notification_service.Models;
using notification_service.Services.Interfaces;

namespace notification_service.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _db;
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hub;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        NotificationDbContext db,
        IHubContext<NotificationsHub, INotificationsClient> hub,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    public async Task<NotificationResponse> CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? entityId = null,
        string? sourceEvent = null,
        CancellationToken cancellationToken = default)
    {
        var entity = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            EntityId = entityId,
            SourceEvent = sourceEvent,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var response = NotificationResponse.FromEntity(entity);
        await _hub.Clients.Group(NotificationsHub.GetGroupName(userId)).NotificationCreated(response);

        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", entity.Id, userId);
        return response;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
        Guid userId,
        bool unreadOnly = false,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(notification => !notification.IsRead);
        }

        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);

        return items.Select(NotificationResponse.FromEntity).ToList();
    }

    public async Task<NotificationStatsResponse> GetStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var total = await _db.Notifications.CountAsync(notification => notification.UserId == userId, cancellationToken);
        var unread = await _db.Notifications.CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);

        return new NotificationStatsResponse
        {
            Total = total,
            Unread = unread
        };
    }

    public async Task<NotificationResponse?> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Notifications
            .FirstOrDefaultAsync(notification => notification.Id == notificationId && notification.UserId == userId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            entity.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        var response = NotificationResponse.FromEntity(entity);
        await _hub.Clients.Group(NotificationsHub.GetGroupName(userId)).NotificationRead(notificationId);

        return response;
    }

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _db.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        var updated = await _db.SaveChangesAsync(cancellationToken);
        await _hub.Clients.Group(NotificationsHub.GetGroupName(userId)).NotificationsReadAll();

        return updated;
    }
}