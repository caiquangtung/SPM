using project_service.DTOs.Tasks;
using TaskStatusEnum = project_service.Models.TaskStatus;

namespace project_service.Services.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetByProjectAsync(
        Guid projectId,
        TaskStatusEnum? status = null,
        Guid? assignedTo = null,
        CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> SearchSimilarAsync(SearchTasksRequest request, CancellationToken cancellationToken = default);
}
