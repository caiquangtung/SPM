import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { projectService } from "@/lib/services/projects";
import { handleApiError } from "@/lib/api-helpers";
import { toast } from "sonner";
import type { Project, CreateProjectRequest } from "@/types/project";

/**
 * Get all projects for the current user
 */
export function useProjects() {
  return useQuery({
    queryKey: ["projects"],
    queryFn: () => projectService.getMyProjects(),
    staleTime: 30000, // 30 seconds
  });
}

/**
 * Get a single project by ID
 */
export function useProject(id: string | undefined) {
  return useQuery({
    queryKey: ["projects", id],
    queryFn: () => projectService.getProjectById(id!),
    enabled: !!id,
    staleTime: 30000,
  });
}

/**
 * Create a new project
 */
export function useCreateProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateProjectRequest) =>
      projectService.createProject(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      toast.success("Project created successfully");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

