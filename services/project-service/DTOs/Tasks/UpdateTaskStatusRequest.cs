using project_service.Models;
using TaskStatusEnum = project_service.Models.TaskStatus;

namespace project_service.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    public TaskStatusEnum Status { get; set; }
}


