using notification_service.DTOs;

namespace notification_service.Services.Interfaces;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(
        Guid userId,
        Models.NotificationType type,
        string title,
        string message,
        string? entityId = null,
        string? sourceEvent = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
        Guid userId,
        bool unreadOnly = false,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<NotificationStatsResponse> GetStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationResponse?> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
}