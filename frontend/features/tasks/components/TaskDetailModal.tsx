"use client";

import { Task, TaskStatus, TaskPriority } from "@/types/project";
import { format } from "date-fns";
import { X, Calendar, User, Tag } from "lucide-react";
import CommentSection from "@/features/comments/components/CommentSection";
import FileUpload from "@/features/files/components/FileUpload";
import { useTaskAttachments } from "@/features/files/hooks";
import { useDetachFileFromTask } from "@/features/files/hooks";
import { Download, Trash2 } from "lucide-react";
import { fileService } from "@/lib/services/files";

interface TaskDetailModalProps {
  task: Task;
  onClose: () => void;
}

const statusColors: Record<TaskStatus, string> = {
  [TaskStatus.ToDo]: "bg-gray-100 text-gray-800",
  [TaskStatus.InProgress]: "bg-blue-100 text-blue-800",
  [TaskStatus.InReview]: "bg-purple-100 text-purple-800",
  [TaskStatus.Done]: "bg-green-100 text-green-800",
  [TaskStatus.Cancelled]: "bg-red-100 text-red-800",
};

const priorityColors: Record<TaskPriority, string> = {
  [TaskPriority.Low]: "bg-blue-100 text-blue-800",
  [TaskPriority.Medium]: "bg-yellow-100 text-yellow-800",
  [TaskPriority.High]: "bg-orange-100 text-orange-800",
  [TaskPriority.Critical]: "bg-red-100 text-red-800",
};

export default function TaskDetailModal({
  task,
  onClose,
}: TaskDetailModalProps) {
  const { data: attachments } = useTaskAttachments(task.id);
  const detachFile = useDetachFileFromTask();

  const handleDownload = async (fileId: string, fileName: string) => {
    try {
      const blob = await fileService.downloadFile(fileId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error("Download failed:", error);
    }
  };

  const handleDetach = async (attachmentId: string) => {
    await detachFile.mutateAsync({
      taskId: task.id,
      attachmentId,
    });
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
          <h2 className="text-2xl font-bold text-gray-900">{task.title}</h2>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        <div className="px-6 py-4 space-y-6">
          {/* Status and Priority */}
          <div className="flex items-center gap-3">
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${statusColors[task.status]}`}
            >
              {task.status}
            </span>
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${priorityColors[task.priority]}`}
            >
              {task.priority}
            </span>
          </div>

          {/* Description */}
          {task.description && (
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">
                Description
              </h3>
              <p className="text-gray-900 whitespace-pre-wrap">
                {task.description}
              </p>
            </div>
          )}

          {/* Metadata */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            {task.dueDate && (
              <div className="flex items-center gap-2 text-gray-600">
                <Calendar className="w-4 h-4" />
                <span>
                  Due: {format(new Date(task.dueDate), "MMM dd, yyyy")}
                </span>
              </div>
            )}
            {task.assignedTo && (
              <div className="flex items-center gap-2 text-gray-600">
                <User className="w-4 h-4" />
                <span>Assigned to: {task.assignedTo}</span>
              </div>
            )}
            <div className="flex items-center gap-2 text-gray-600">
              <Tag className="w-4 h-4" />
              <span>
                Created: {format(new Date(task.createdAt), "MMM dd, yyyy")}
              </span>
            </div>
          </div>

          {/* Attachments */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">
              Attachments
            </h3>
            <FileUpload taskId={task.id} />
            {attachments && attachments.length > 0 && (
              <div className="mt-4 space-y-2">
                {attachments.map((attachment) => (
                  <div
                    key={attachment.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200"
                  >
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-gray-900">
                        {attachment.file.originalName}
                      </span>
                      <span className="text-xs text-gray-500">
                        ({(attachment.file.size / 1024).toFixed(2)} KB)
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() =>
                          handleDownload(
                            attachment.file.id,
                            attachment.file.originalName
                          )
                        }
                        className="p-1 text-gray-600 hover:text-blue-600"
                      >
                        <Download className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDetach(attachment.id)}
                        className="p-1 text-gray-600 hover:text-red-600"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Comments */}
          <div>
            <CommentSection taskId={task.id} />
          </div>
        </div>
      </div>
    </div>
  );
}

