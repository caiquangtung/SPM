"use client";

import { Task, TaskPriority, TaskStatus } from "@/types/project";
import { format } from "date-fns";
import { Calendar, User, AlertCircle } from "lucide-react";

interface TaskCardProps {
  task: Task;
  onClick?: () => void;
}

const priorityColors: Record<TaskPriority, string> = {
  [TaskPriority.Low]: "bg-blue-100 text-blue-800",
  [TaskPriority.Medium]: "bg-yellow-100 text-yellow-800",
  [TaskPriority.High]: "bg-orange-100 text-orange-800",
  [TaskPriority.Critical]: "bg-red-100 text-red-800",
};

const statusColors: Record<TaskStatus, string> = {
  [TaskStatus.ToDo]: "bg-gray-100 text-gray-800",
  [TaskStatus.InProgress]: "bg-blue-100 text-blue-800",
  [TaskStatus.InReview]: "bg-purple-100 text-purple-800",
  [TaskStatus.Done]: "bg-green-100 text-green-800",
  [TaskStatus.Cancelled]: "bg-red-100 text-red-800",
};

export default function TaskCard({ task, onClick }: TaskCardProps) {
  const isOverdue =
    task.dueDate && new Date(task.dueDate) < new Date() && task.status !== TaskStatus.Done;

  return (
    <div
      onClick={onClick}
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-4 cursor-pointer hover:shadow-md transition-shadow ${
        isOverdue ? "border-red-300" : ""
      }`}
    >
      <div className="flex items-start justify-between mb-2">
        <h3 className="font-semibold text-gray-900 text-sm line-clamp-2">
          {task.title}
        </h3>
        {isOverdue && (
          <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 ml-2" />
        )}
      </div>

      {task.description && (
        <p className="text-xs text-gray-600 mb-3 line-clamp-2">
          {task.description}
        </p>
      )}

      <div className="flex items-center gap-2 flex-wrap">
        <span
          className={`px-2 py-1 rounded text-xs font-medium ${statusColors[task.status]}`}
        >
          {task.status}
        </span>
        <span
          className={`px-2 py-1 rounded text-xs font-medium ${priorityColors[task.priority]}`}
        >
          {task.priority}
        </span>
      </div>

      <div className="mt-3 flex items-center gap-4 text-xs text-gray-500">
        {task.dueDate && (
          <div className="flex items-center gap-1">
            <Calendar className="w-3 h-3" />
            <span className={isOverdue ? "text-red-600 font-medium" : ""}>
              {format(new Date(task.dueDate), "MMM dd")}
            </span>
          </div>
        )}
        {task.assignedTo && (
          <div className="flex items-center gap-1">
            <User className="w-3 h-3" />
            <span>Assigned</span>
          </div>
        )}
      </div>
    </div>
  );
}

