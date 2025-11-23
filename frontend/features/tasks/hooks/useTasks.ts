import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { taskService } from "@/lib/services/tasks";
import { handleApiError } from "@/lib/api-helpers";
import { toast } from "sonner";
import type {
  Task,
  CreateTaskRequest,
  UpdateTaskStatusRequest,
  GetTasksQuery,
} from "@/types/project";

/**
 * Get tasks by project with optional filters
 */
export function useTasks(query: GetTasksQuery) {
  return useQuery({
    queryKey: ["tasks", query.projectId, query.status, query.assignedTo],
    queryFn: () => taskService.getTasksByProject(query),
    enabled: !!query.projectId,
    staleTime: 10000, // 10 seconds
  });
}

/**
 * Create a new task
 */
export function useCreateTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateTaskRequest) =>
      taskService.createTask(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["tasks", variables.projectId],
      });
      toast.success("Task created successfully");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

/**
 * Update task status
 */
export function useUpdateTaskStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      request,
    }: {
      taskId: string;
      request: UpdateTaskStatusRequest;
    }) => taskService.updateTaskStatus(taskId, request),
    onSuccess: (task) => {
      queryClient.invalidateQueries({
        queryKey: ["tasks", task.projectId],
      });
      queryClient.invalidateQueries({
        queryKey: ["tasks", task.id],
      });
      toast.success("Task status updated");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

/**
 * Search similar tasks
 */
export function useSearchTasks() {
  return useMutation({
    mutationFn: (query: string) => taskService.searchSimilarTasks(query),
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

