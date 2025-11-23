"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useProject } from "@/features/projects/hooks";
import { useTasks, useCreateTask } from "@/features/tasks/hooks";
import { KanbanBoard, TaskDetailModal } from "@/features/tasks/components";
import { TaskForm } from "@/features/tasks/components";
import { Task } from "@/types/project";
import { Plus, List, Loader2 } from "lucide-react";
import Link from "next/link";

export default function ProjectKanbanPage() {
  const params = useParams();
  const router = useRouter();
  const projectId = params.id as string;

  const { data: project, isLoading: projectLoading } = useProject(projectId);
  const { data: tasks, isLoading: tasksLoading } = useTasks({
    projectId,
  });
  const createTask = useCreateTask();

  const [showTaskForm, setShowTaskForm] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  if (projectLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!project) {
    return (
      <div className="container mx-auto px-4 py-8">
        <p className="text-red-600">Project not found</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{project.name}</h1>
          {project.description && (
            <p className="text-gray-600 mt-1">{project.description}</p>
          )}
        </div>
        <div className="flex items-center gap-2">
          <Link
            href={`/projects/${projectId}/list`}
            className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <List className="w-5 h-5" />
            List View
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
      ) : (
        <KanbanBoard
          tasks={tasks || []}
          onTaskClick={(task) => setSelectedTask(task)}
        />
      )}

      {selectedTask && (
        <TaskDetailModal
          task={selectedTask}
          onClose={() => setSelectedTask(null)}
        />
      )}
    </div>
  );
}

