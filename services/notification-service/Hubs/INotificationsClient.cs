using notification_service.DTOs;

namespace notification_service.Hubs;

public interface INotificationsClient
{
    Task NotificationCreated(NotificationResponse notification);
    Task NotificationUpdated(NotificationResponse notification);
    Task NotificationsReadAll();
    Task NotificationRead(Guid notificationId);
}