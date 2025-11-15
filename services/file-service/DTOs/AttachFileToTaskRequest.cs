namespace file_service.DTOs;

public class AttachFileToTaskRequest
{
    public Guid FileId { get; set; }
    public Guid TaskId { get; set; }
}

