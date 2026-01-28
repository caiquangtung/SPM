"use client";

import { useState, useEffect } from "react";
import { DragDropContext, Droppable, Draggable, DropResult } from "@hello-pangea/dnd";
import { Task, TaskStatus } from "@/types/project";
import { useUpdateTaskStatus } from "../hooks";
import TaskCard from "./TaskCard";

interface KanbanBoardProps {
  tasks: Task[];
  onTaskClick?: (task: Task) => void;
}

const columns: { id: TaskStatus; title: string }[] = [
  { id: TaskStatus.ToDo, title: "To Do" },
  { id: TaskStatus.InProgress, title: "In Progress" },
  { id: TaskStatus.InReview, title: "In Review" },
  { id: TaskStatus.Done, title: "Done" },
];

export default function KanbanBoard({
  tasks,
  onTaskClick,
}: KanbanBoardProps) {
  const updateTaskStatus = useUpdateTaskStatus();
  const [mounted, setMounted] = useState(false);

  // Prevent SSR/hydration mismatch - render DnD only on client
  useEffect(() => {
    setMounted(true);
  }, []);

  const tasksByStatus = columns.reduce((acc, column) => {
    acc[column.id] = tasks.filter((task) => task.status === column.id);
    return acc;
  }, {} as Record<TaskStatus, Task[]>);

  // Don't render DnD until client-side mount
  if (!mounted) {
    return (
      <div className="flex gap-4 pb-4">
        {columns.map((column) => (
          <div
            key={column.id}
            className="flex-shrink-0 w-80 bg-gray-50 rounded-lg p-4"
          >
            <h3 className="font-semibold text-gray-900 mb-4">
              {column.title} ({tasksByStatus[column.id]?.length || 0})
            </h3>
            <div className="min-h-[200px] space-y-2 p-2  border-dashed border-gray-200 bg-white">
              {tasksByStatus[column.id]?.map((task) => (
                <TaskCard
                  key={task.id}
                  task={task}
                  onClick={() => onTaskClick?.(task)}
                />
              ))}
            </div>
          </div>
        ))}
      </div>
    );
  }

  const handleDragEnd = (result: DropResult) => {
    const { destination, source, draggableId } = result;

    // Dropped outside the list
    if (!destination) return;
    
    // Dropped in the same position
    if (
      destination.droppableId === source.droppableId &&
      destination.index === source.index
    ) {
      return;
    }

    const newStatus = destination.droppableId as TaskStatus;
    const task = tasks.find((t) => t.id === draggableId);

    // Only update if status actually changed
    if (task && task.status !== newStatus) {
      updateTaskStatus.mutate({
        taskId: draggableId,
        request: { status: newStatus },
      });
    }
  };

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <div className="flex gap-4 pb-4">
        {columns.map((column) => (
          <div
            key={column.id}
            className="flex-shrink-0 w-80 bg-gray-50 rounded-lg p-4"
          >
            <h3 className="font-semibold text-gray-900 mb-4">
              {column.title} ({tasksByStatus[column.id]?.length || 0})
            </h3>
            <Droppable droppableId={column.id}>
              {(provided, snapshot) => (
                <div
                  ref={provided.innerRef}
                  {...provided.droppableProps}
                  className={`min-h-[200px] space-y-2 p-2 rounded-lg border-2 border-dashed transition-colors ${
                    snapshot.isDraggingOver
                      ? "border-blue-400 bg-blue-50"
                      : "border-gray-200 bg-white"
                  }`}
                >
                  {tasksByStatus[column.id]?.map((task, index) => (
                    <Draggable
                      key={task.id}
                      draggableId={task.id}
                      index={index}
                    >
                      {(provided, snapshot) => (
                        <div
                          ref={provided.innerRef}
                          {...provided.draggableProps}
                          {...provided.dragHandleProps}
                          className={snapshot.isDragging ? "opacity-75" : ""}
                        >
                          <TaskCard
                            task={task}
                            onClick={() => onTaskClick?.(task)}
                          />
                        </div>
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </div>
        ))}
      </div>
    </DragDropContext>
  );
}

