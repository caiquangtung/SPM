"use client";

import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { TaskPriority, CreateTaskRequest } from "@/types/project";
import { useCreateTask } from "../hooks";
import ControlledInput from "@/components/forms/ControlledInput";

const taskSchema = z.object({
  title: z.string().min(1, "Title is required").max(200, "Title too long"),
  description: z.string().max(1000, "Description too long").optional(),
  priority: z.nativeEnum(TaskPriority),
  dueDate: z.string().optional(),
  assignedTo: z.string().uuid().optional().or(z.literal("")),
});

type TaskFormData = z.infer<typeof taskSchema> & {
  projectId: string;
};

interface TaskFormProps {
  projectId: string;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export default function TaskForm({
  projectId,
  onSuccess,
  onCancel,
}: TaskFormProps) {
  const createTask = useCreateTask();

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<TaskFormData>({
    resolver: zodResolver(taskSchema),
    defaultValues: {
      title: "",
      description: "",
      priority: TaskPriority.Medium,
      dueDate: "",
      assignedTo: "",
    },
  });

  const onSubmit = async (data: TaskFormData) => {
    const request: CreateTaskRequest = {
      projectId,
      title: data.title,
      description: data.description || undefined,
      priority: data.priority,
      dueDate: data.dueDate ? new Date(data.dueDate).toISOString() : undefined,
      assignedTo: data.assignedTo || undefined,
    };

    await createTask.mutateAsync(request);
    onSuccess?.();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <ControlledInput
        control={control}
        name="title"
        label="Title"
        placeholder="Enter task title"
      />

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <textarea
              {...field}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
              placeholder="Enter task description"
            />
          )}
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-600">
            {errors.description.message}
          </p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Priority
        </label>
        <Controller
          control={control}
          name="priority"
          render={({ field }) => (
            <select
              {...field}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
            >
              {Object.values(TaskPriority).map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </select>
          )}
        />
      </div>

      <ControlledInput
        control={control}
        name="dueDate"
        label="Due Date"
        type="datetime-local"
      />

      <div className="flex gap-2 justify-end">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Cancel
          </button>
        )}
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
        >
          {isSubmitting ? "Creating..." : "Create Task"}
        </button>
      </div>
    </form>
  );
}

