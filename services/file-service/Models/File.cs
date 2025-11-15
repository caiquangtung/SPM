namespace file_service.Models;

public class File
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalName { get; set; } = string.Empty;
    public string StoredName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();
}

