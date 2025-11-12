namespace project_service.Services.Interfaces;

public interface IKafkaProducerService
{
    Task PublishProjectCreatedAsync(Guid projectId, Guid createdBy, string name);
    Task PublishProjectUpdatedAsync(Guid projectId, Guid updatedBy, string name);
    Task PublishTaskCreatedAsync(Guid taskId, Guid projectId, Guid createdBy, string title);
    Task PublishTaskUpdatedAsync(Guid taskId, Guid projectId, Guid updatedBy, string title);
    Task PublishTaskStatusChangedAsync(Guid taskId, Guid projectId, string oldStatus, string newStatus);
    Task PublishCommentCreatedAsync(Guid commentId, Guid taskId, Guid userId, string content);
}

