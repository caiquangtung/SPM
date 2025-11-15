using file_service.Models;

namespace file_service.Repositories.Interfaces;

public interface ITaskAttachmentRepository
{
    Task<TaskAttachment?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(Guid taskId);
    Task<TaskAttachment> CreateAsync(TaskAttachment attachment);
    Task<bool> DeleteAsync(Guid id);
}

