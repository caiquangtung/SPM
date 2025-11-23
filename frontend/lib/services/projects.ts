import apiClient from "../axios";
import { unwrapResponse } from "../api-helpers";
import type {
  Project,
  CreateProjectRequest,
  ApiResponse,
} from "@/types/project";

export const projectService = {
  /**
   * Get all projects for the current user
   */
  async getMyProjects(): Promise<Project[]> {
    const response = await apiClient.get<ApiResponse<Project[]>>("/projects");
    return unwrapResponse(response);
  },

  /**
   * Get project by ID
   */
  async getProjectById(id: string): Promise<Project> {
    const response = await apiClient.get<ApiResponse<Project>>(
      `/projects/${id}`
    );
    return unwrapResponse(response);
  },

  /**
   * Create a new project
   */
  async createProject(request: CreateProjectRequest): Promise<Project> {
    const response = await apiClient.post<ApiResponse<Project>>(
      "/projects",
      request
    );
    return unwrapResponse(response);
  },
};
