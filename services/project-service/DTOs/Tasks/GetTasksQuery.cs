using TaskStatusEnum = project_service.Models.TaskStatus;

namespace project_service.DTOs.Tasks;

public class GetTasksQuery
{
    public Guid ProjectId { get; set; }
    public TaskStatusEnum? Status { get; set; }
    public Guid? AssignedTo { get; set; }
}
