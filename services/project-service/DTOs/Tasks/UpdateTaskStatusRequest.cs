using project_service.Models;

namespace project_service.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}


