import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { commentService } from "@/lib/services/comments";
import { handleApiError } from "@/lib/api-helpers";
import { toast } from "sonner";
import type { CreateCommentRequest } from "@/types/project";

/**
 * Get comments for a task
 */
export function useComments(taskId: string | undefined) {
  return useQuery({
    queryKey: ["comments", taskId],
    queryFn: () => commentService.getCommentsByTask(taskId!),
    enabled: !!taskId,
    staleTime: 10000,
  });
}

/**
 * Create a new comment
 */
export function useCreateComment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      request,
    }: {
      taskId: string;
      request: CreateCommentRequest;
    }) => commentService.createComment(taskId, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["comments", variables.taskId],
      });
      toast.success("Comment added");
    },
    onError: (error) => {
      const apiError = handleApiError(error);
      toast.error(apiError.message);
    },
  });
}

