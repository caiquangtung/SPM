using file_service.Models;

namespace file_service.DTOs;

public class TaskAttachmentResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid FileId { get; set; }
    public FileResponse File { get; set; } = null!;
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }

    public static TaskAttachmentResponse FromEntity(TaskAttachment attachment)
    {
        return new TaskAttachmentResponse
        {
            Id = attachment.Id,
            TaskId = attachment.TaskId,
            FileId = attachment.FileId,
            File = FileResponse.FromEntity(attachment.File),
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt
        };
    }
}

