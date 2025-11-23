import apiClient from "../axios";
import { unwrapResponse } from "../api-helpers";
import type {
  Comment,
  CreateCommentRequest,
  ApiResponse,
} from "@/types/project";

export const commentService = {
  /**
   * Get comments for a task
   */
  async getCommentsByTask(taskId: string): Promise<Comment[]> {
    const response = await apiClient.get<ApiResponse<Comment[]>>(
      `/tasks/${taskId}/comments`
    );
    return unwrapResponse(response);
  },

  /**
   * Create a new comment
   */
  async createComment(
    taskId: string,
    request: CreateCommentRequest
  ): Promise<Comment> {
    const response = await apiClient.post<ApiResponse<Comment>>(
      `/tasks/${taskId}/comments`,
      request
    );
    return unwrapResponse(response);
  },
};
