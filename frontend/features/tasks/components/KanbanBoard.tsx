"use client";

import { DragDropContext, Droppable, Draggable, DropResult } from "react-beautiful-dnd";
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

  const tasksByStatus = columns.reduce((acc, column) => {
    acc[column.id] = tasks.filter((task) => task.status === column.id);
    return acc;
  }, {} as Record<TaskStatus, Task[]>);

  const handleDragEnd = (result: DropResult) => {
    const { destination, source, draggableId } = result;

    if (!destination) return;
    if (
      destination.droppableId === source.droppableId &&
      destination.index === source.index
    ) {
      return;
    }

    const newStatus = destination.droppableId as TaskStatus;
    const task = tasks.find((t) => t.id === draggableId);

    if (task && task.status !== newStatus) {
      updateTaskStatus.mutate({
        taskId: draggableId,
        request: { status: newStatus },
      });
    }
  };

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <div className="flex gap-4 overflow-x-auto pb-4">
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
                  className={`min-h-[200px] space-y-2 ${
                    snapshot.isDraggingOver ? "bg-blue-50" : ""
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

