import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { fileService } from "@/lib/services/files";
import { handleApiError } from "@/lib/api-helpers";
import { toast } from "sonner";

/**
 * Get all files uploaded by current user
 */
export function useFiles() {
  return useQuery({
    queryKey: ["files"],
    queryFn: () => fileService.getMyFiles(),
    staleTime: 30000,
  });
}

/**
 * Upload a file
 */
export function useUploadFile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => fileService.uploadFile(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["files"] });
      toast.success("File uploaded successfully");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

/**
 * Get task attachments
 */
export function useTaskAttachments(taskId: string | undefined) {
  return useQuery({
    queryKey: ["task-attachments", taskId],
    queryFn: () => fileService.getTaskAttachments(taskId!),
    enabled: !!taskId,
    staleTime: 10000,
  });
}

/**
 * Attach file to task
 */
export function useAttachFileToTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      fileId,
    }: {
      taskId: string;
      fileId: string;
    }) => fileService.attachFileToTask(taskId, fileId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["task-attachments", variables.taskId],
      });
      toast.success("File attached to task");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

/**
 * Detach file from task
 */
export function useDetachFileFromTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      attachmentId,
    }: {
      taskId: string;
      attachmentId: string;
    }) => fileService.detachFileFromTask(taskId, attachmentId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["task-attachments", variables.taskId],
      });
      toast.success("File detached from task");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

