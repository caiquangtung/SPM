import type { ApiResponse } from "@/types/project";

export enum NotificationType {
  ProjectCreated = "ProjectCreated",
  ProjectUpdated = "ProjectUpdated",
  TaskCreated = "TaskCreated",
  TaskUpdated = "TaskUpdated",
  TaskAssigned = "TaskAssigned",
  TaskStatusChanged = "TaskStatusChanged",
  CommentCreated = "CommentCreated",
  System = "System",
}

export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  entityId?: string | null;
  sourceEvent?: string | null;
  isRead: boolean;
  createdAt: string;
  readAt?: string | null;
}

export interface NotificationStats {
  total: number;
  unread: number;
}

export type NotificationListResponse = ApiResponse<Notification[]>;
export type NotificationStatsResponse = ApiResponse<NotificationStats>;
