import apiClient from "../axios";
import { unwrapResponse } from "../api-helpers";
import type {
  Task,
  CreateTaskRequest,
  UpdateTaskStatusRequest,
  GetTasksQuery,
  ApiResponse,
} from "@/types/project";

export const taskService = {
  /**
   * Get tasks by project with optional filters
   */
  async getTasksByProject(query: GetTasksQuery): Promise<Task[]> {
    const params = new URLSearchParams();
    params.append("projectId", query.projectId);
    if (query.status) {
      params.append("status", query.status);
    }
    if (query.assignedTo) {
      params.append("assignedTo", query.assignedTo);
    }

    const response = await apiClient.get<ApiResponse<Task[]>>(
      `/tasks?${params.toString()}`
    );
    return unwrapResponse(response);
  },

  /**
   * Create a new task
   */
  async createTask(request: CreateTaskRequest): Promise<Task> {
    const response = await apiClient.post<ApiResponse<Task>>("/tasks", request);
    return unwrapResponse(response);
  },

  /**
   * Update task status
   */
  async updateTaskStatus(
    taskId: string,
    request: UpdateTaskStatusRequest
  ): Promise<Task> {
    const response = await apiClient.put<ApiResponse<Task>>(
      `/tasks/${taskId}/status`,
      request
    );
    return unwrapResponse(response);
  },

  /**
   * Search similar tasks using semantic search
   */
  async searchSimilarTasks(query: string): Promise<Task[]> {
    const response = await apiClient.post<ApiResponse<Task[]>>(
      "/tasks/search",
      { query }
    );
    return unwrapResponse(response);
  },
};
