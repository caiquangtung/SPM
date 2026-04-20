using notification_service.Models;

namespace notification_service.DTOs;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? SourceEvent { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public static NotificationResponse FromEntity(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            EntityId = notification.EntityId,
            SourceEvent = notification.SourceEvent,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}

public class NotificationStatsResponse
{
    public int Total { get; set; }
    public int Unread { get; set; }
}