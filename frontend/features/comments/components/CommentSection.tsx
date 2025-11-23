"use client";

import { useComments, useCreateComment } from "../hooks";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { MessageSquare, Send } from "lucide-react";
import { format } from "date-fns";

const commentSchema = z.object({
  content: z.string().min(1, "Comment cannot be empty").max(1000),
});

type CommentFormData = z.infer<typeof commentSchema>;

interface CommentSectionProps {
  taskId: string;
}

export default function CommentSection({ taskId }: CommentSectionProps) {
  const { data: comments, isLoading } = useComments(taskId);
  const createComment = useCreateComment();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CommentFormData>({
    resolver: zodResolver(commentSchema),
  });

  const onSubmit = async (data: CommentFormData) => {
    await createComment.mutateAsync({
      taskId,
      request: { content: data.content },
    });
    reset();
  };

  if (isLoading) {
    return <div className="text-sm text-gray-500">Loading comments...</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <MessageSquare className="w-5 h-5 text-gray-600" />
        <h3 className="text-lg font-semibold text-gray-900">
          Comments ({comments?.length || 0})
        </h3>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-2">
        <div>
          <textarea
            {...register("content")}
            rows={3}
            placeholder="Add a comment..."
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
          />
          {errors.content && (
            <p className="mt-1 text-sm text-red-600">
              {errors.content.message}
            </p>
          )}
        </div>
        <button
          type="submit"
          disabled={isSubmitting}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
        >
          <Send className="w-4 h-4" />
          {isSubmitting ? "Posting..." : "Post Comment"}
        </button>
      </form>

      <div className="space-y-3 mt-4">
        {comments && comments.length > 0 ? (
          comments.map((comment) => (
            <div
              key={comment.id}
              className="bg-gray-50 rounded-lg p-3 border border-gray-200"
            >
              <p className="text-sm text-gray-900">{comment.content}</p>
              <p className="mt-2 text-xs text-gray-500">
                {format(new Date(comment.createdAt), "MMM dd, yyyy HH:mm")}
              </p>
            </div>
          ))
        ) : (
          <p className="text-sm text-gray-500">No comments yet</p>
        )}
      </div>
    </div>
  );
}

