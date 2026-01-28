"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import { AppLayout } from "@/components/layout";
import { useProject } from "@/features/projects/hooks";
import { useTasks } from "@/features/tasks/hooks";
import {
  TaskCard,
  TaskDetailModal,
  TaskForm,
} from "@/features/tasks/components";
import { Task, TaskStatus } from "@/types/project";
import { Plus, Columns, Loader2, Filter } from "lucide-react";
import Link from "next/link";

export default function ProjectListPage() {
  const params = useParams();
  const projectId = (params?.id as string) || "";

  const { data: project, isLoading: projectLoading } = useProject(projectId);
  const [statusFilter, setStatusFilter] = useState<TaskStatus | undefined>();
  const { data: tasks, isLoading: tasksLoading } = useTasks({
    projectId,
    status: statusFilter,
  });

  const [showTaskForm, setShowTaskForm] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  if (projectLoading) {
    return (
      <AppLayout>
        <div className="flex items-center justify-center min-h-screen">
          <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      </AppLayout>
    );
  }

  if (!project) {
    return (
      <AppLayout>
        <div className="p-6 sm:p-8">
          <p className="text-red-600">Project not found</p>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <div className="p-6 sm:p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{project.name}</h1>
          {project.description && (
            <p className="text-gray-600 mt-1">{project.description}</p>
          )}
        </div>
        <div className="flex items-center gap-2">
          <Link
            href={`/projects/${projectId}`}
            className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Columns className="w-5 h-5" />
            Kanban View
          </Link>
          <button
            onClick={() => setShowTaskForm(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-5 h-5" />
            New Task
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="mb-6 flex items-center gap-4">
        <div className="flex items-center gap-2">
          <Filter className="w-5 h-5 text-gray-600" />
          <span className="text-sm font-medium text-gray-700">Filter:</span>
        </div>
        <select
          value={statusFilter || ""}
          onChange={(e) =>
            setStatusFilter(
              e.target.value ? (e.target.value as TaskStatus) : undefined
            )
          }
          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="">All Status</option>
          {Object.values(TaskStatus).map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>
      </div>

      {showTaskForm && (
        <div className="mb-6 p-6 bg-white rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-xl font-semibold mb-4">Create New Task</h2>
          <TaskForm
            projectId={projectId}
            onSuccess={() => {
              setShowTaskForm(false);
            }}
            onCancel={() => setShowTaskForm(false)}
          />
        </div>
      )}

      {tasksLoading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      ) : tasks && tasks.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {tasks.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onClick={() => setSelectedTask(task)}
            />
          ))}
        </div>
      ) : (
        <div className="text-center py-12">
          <p className="text-gray-600">No tasks found</p>
          <p className="text-sm text-gray-500 mt-2">
            Create your first task to get started
          </p>
        </div>
      )}

      {selectedTask && (
        <TaskDetailModal
          task={selectedTask}
          onClose={() => setSelectedTask(null)}
        />
      )}
      </div>
    </AppLayout>
  );
}
