using file_service.DTOs;

namespace file_service.Services.Interfaces;

public interface ITaskAttachmentService
{
    Task<TaskAttachmentResponse> AttachFileToTaskAsync(Guid userId, Guid taskId, Guid fileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskAttachmentResponse>> GetTaskAttachmentsAsync(Guid taskId);
    Task<bool> DetachFileFromTaskAsync(Guid attachmentId, Guid userId);
}

