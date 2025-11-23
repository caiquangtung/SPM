import apiClient from "../axios";
import { unwrapResponse } from "../api-helpers";
import type { File, TaskAttachment, ApiResponse } from "@/types/project";

export const fileService = {
  /**
   * Upload a file
   */
  async uploadFile(file: globalThis.File): Promise<File> {
    const formData = new FormData();
    formData.append("file", file);

    const response = await apiClient.post<ApiResponse<File>>(
      "/files/upload",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );
    return unwrapResponse(response);
  },

  /**
   * Get file metadata by ID
   */
  async getFileById(id: string): Promise<File> {
    const response = await apiClient.get<ApiResponse<File>>(`/files/${id}`);
    return unwrapResponse(response);
  },

  /**
   * Download file content
   */
  async downloadFile(id: string): Promise<Blob> {
    const response = await apiClient.get(`/files/${id}/download`, {
      responseType: "blob",
    });
    return response.data;
  },

  /**
   * Delete a file
   */
  async deleteFile(id: string): Promise<void> {
    await apiClient.delete(`/files/${id}`);
  },

  /**
   * Get all files uploaded by current user
   */
  async getMyFiles(): Promise<File[]> {
    const response = await apiClient.get<ApiResponse<File[]>>(
      "/files/my-files"
    );
    return unwrapResponse(response);
  },

  /**
   * Attach file to task
   */
  async attachFileToTask(
    taskId: string,
    fileId: string
  ): Promise<TaskAttachment> {
    const response = await apiClient.post<ApiResponse<TaskAttachment>>(
      `/tasks/${taskId}/attachments`,
      { taskId, fileId }
    );
    return unwrapResponse(response);
  },

  /**
   * Get task attachments
   */
  async getTaskAttachments(taskId: string): Promise<TaskAttachment[]> {
    const response = await apiClient.get<ApiResponse<TaskAttachment[]>>(
      `/tasks/${taskId}/attachments`
    );
    return unwrapResponse(response);
  },

  /**
   * Detach file from task
   */
  async detachFileFromTask(
    taskId: string,
    attachmentId: string
  ): Promise<void> {
    await apiClient.delete(`/tasks/${taskId}/attachments/${attachmentId}`);
  },
};
