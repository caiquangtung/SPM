export enum TaskStatus {
  ToDo = "ToDo",
  InProgress = "InProgress",
  InReview = "InReview",
  Done = "Done",
  Cancelled = "Cancelled",
}

export enum TaskPriority {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical",
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

export interface Task {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  assignedTo?: string;
  createdBy: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Comment {
  id: string;
  taskId: string;
  userId: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}

export interface File {
  id: string;
  originalName: string;
  mimeType: string;
  size: number;
  uploadedBy: string;
  uploadedAt: string;
}

export interface TaskAttachment {
  id: string;
  taskId: string;
  fileId: string;
  file: File;
  uploadedBy: string;
  uploadedAt: string;
}

// Request DTOs
export interface CreateProjectRequest {
  name: string;
  description?: string;
}

export interface CreateTaskRequest {
  projectId: string;
  title: string;
  description?: string;
  assignedTo?: string;
  priority: TaskPriority;
  dueDate?: string;
}

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}

export interface CreateCommentRequest {
  content: string;
}

export interface GetTasksQuery {
  projectId: string;
  status?: TaskStatus;
  assignedTo?: string;
}

// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errorCode?: string;
  timestamp: string;
}

export interface ApiResponseSimple {
  success: boolean;
  message: string;
  errorCode?: string;
  timestamp: string;
}

