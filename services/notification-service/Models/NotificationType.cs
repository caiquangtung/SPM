namespace notification_service.Models;

public enum NotificationType
{
    ProjectCreated,
    ProjectUpdated,
    TaskCreated,
    TaskUpdated,
    TaskAssigned,
    TaskStatusChanged,
    CommentCreated,
    System
}