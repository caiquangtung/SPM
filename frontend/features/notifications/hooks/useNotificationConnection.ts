"use client";

import { useEffect } from "react";
import {
  HubConnectionBuilder,
  HttpTransportType,
  LogLevel,
} from "@microsoft/signalr";
import Cookies from "js-cookie";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/contexts/AuthContext";
import { notificationKeys } from "@/lib/services/notifications";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5010";

export function useNotificationConnection() {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    const token = Cookies.get("access_token");
    if (!token) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, {
        accessTokenFactory: () => Cookies.get("access_token") || token,
        transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("NotificationCreated", () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    });

    connection.on("NotificationRead", () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    });

    connection.on("NotificationsReadAll", () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    });

    connection.start().catch(() => {
      // The UI continues to work via polling if the hub is temporarily unavailable.
    });

    return () => {
      connection.stop().catch(() => undefined);
    };
  }, [isAuthenticated, queryClient]);
}
