import apiClient from "@/lib/axios";
import { unwrapResponse } from "@/lib/api-helpers";
import type { ApiResponse } from "@/types/project";
import type { Notification, NotificationStats } from "@/types/notification";

export interface NotificationQueryParams {
  unreadOnly?: boolean;
  take?: number;
}

export const notificationService = {
  async getNotifications(params: NotificationQueryParams = {}) {
    const response = await apiClient.get<ApiResponse<Notification[]>>(
      "/notifications",
      {
        params: {
          unreadOnly: params.unreadOnly,
          take: params.take ?? 50,
        },
      },
    );

    return unwrapResponse(response);
  },

  async getUnreadStats() {
    const response = await apiClient.get<ApiResponse<NotificationStats>>(
      "/notifications/unread-count",
    );
    return unwrapResponse(response);
  },

  async markAsRead(notificationId: string) {
    const response = await apiClient.put<ApiResponse<Notification>>(
      `/notifications/${notificationId}/read`,
    );

    return unwrapResponse(response);
  },

  async markAllAsRead() {
    const response = await apiClient.put<ApiResponse<{ updated: number }>>(
      "/notifications/read-all",
    );
    return unwrapResponse(response);
  },
};

export const notificationKeys = {
  all: ["notifications"] as const,
  list: (params: NotificationQueryParams = {}) =>
    ["notifications", params] as const,
  stats: ["notifications", "stats"] as const,
};
