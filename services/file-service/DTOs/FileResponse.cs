using FileModel = file_service.Models.File;

namespace file_service.DTOs;

public class FileResponse
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }

    public static FileResponse FromEntity(FileModel file)
    {
        return new FileResponse
        {
            Id = file.Id,
            OriginalName = file.OriginalName,
            MimeType = file.MimeType,
            Size = file.Size,
            UploadedBy = file.UploadedBy,
            UploadedAt = file.UploadedAt
        };
    }
}

