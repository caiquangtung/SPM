using FileModel = file_service.Models.File;

namespace file_service.Models;

public class TaskAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public Guid FileId { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public FileModel File { get; set; } = null!;
}

