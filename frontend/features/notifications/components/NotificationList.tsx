"use client";

import { formatDistanceToNow } from "date-fns";
import { CheckCheck, Inbox, Loader2 } from "lucide-react";
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
  useNotificationStats,
} from "../hooks/useNotifications";
import type { Notification } from "@/types/notification";

function formatRelativeTime(isoDate: string) {
  return formatDistanceToNow(new Date(isoDate), { addSuffix: true });
}

interface NotificationListProps {
  compact?: boolean;
}

export function NotificationList({ compact = false }: NotificationListProps) {
  const { data: notifications = [], isLoading } = useNotifications({
    take: compact ? 8 : 50,
  });
  const { data: stats } = useNotificationStats();
  const markAllAsRead = useMarkAllNotificationsRead();
  const markAsRead = useMarkNotificationRead();

  const handleMarkRead = (notification: Notification) => {
    if (!notification.isRead) {
      markAsRead.mutate(notification.id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 rounded-3xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-900/5 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.24em] text-slate-500">
            Activity feed
          </p>
          <h1 className="mt-2 text-2xl font-semibold text-slate-950">
            Notifications
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            {stats?.unread ?? 0} unread notification
            {(stats?.unread ?? 0) === 1 ? "" : "s"}
          </p>
        </div>

        <button
          type="button"
          onClick={() => markAllAsRead.mutate()}
          disabled={(stats?.unread ?? 0) === 0 || markAllAsRead.isPending}
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-slate-950 px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-slate-800 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400"
        >
          {markAllAsRead.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <CheckCheck className="h-4 w-4" />
          )}
          Mark all read
        </button>
      </div>

      <div className="rounded-3xl border border-slate-200 bg-white shadow-sm shadow-slate-900/5 overflow-hidden">
        {isLoading ? (
          <div className="flex items-center justify-center px-6 py-16 text-slate-500">
            <Loader2 className="h-5 w-5 animate-spin" />
            <span className="ml-3 text-sm">Loading notifications...</span>
          </div>
        ) : notifications.length === 0 ? (
          <div className="px-6 py-16 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 text-slate-500">
              <Inbox className="h-7 w-7" />
            </div>
            <h2 className="text-lg font-semibold text-slate-950">
              No notifications yet
            </h2>
            <p className="mt-2 text-sm text-slate-500">
              You will see project activity, task updates, and comments here.
            </p>
          </div>
        ) : (
          <div className="divide-y divide-slate-100">
            {notifications.map((notification) => (
              <button
                key={notification.id}
                type="button"
                onClick={() => handleMarkRead(notification)}
                className={`w-full px-6 py-5 text-left transition-colors hover:bg-slate-50 ${
                  notification.isRead ? "bg-white" : "bg-blue-50/40"
                }`}
              >
                <div className="flex items-start gap-4">
                  <div
                    className={`mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl ${
                      notification.isRead
                        ? "bg-slate-100 text-slate-500"
                        : "bg-blue-100 text-blue-700"
                    }`}
                  >
                    <CheckCheck className="h-5 w-5" />
                  </div>

                  <div className="min-w-0 flex-1">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="text-sm font-semibold text-slate-950">
                          {notification.title}
                        </p>
                        <p className="mt-1 text-sm leading-6 text-slate-600">
                          {notification.message}
                        </p>
                      </div>
                      {!notification.isRead && (
                        <span className="mt-1 h-2.5 w-2.5 rounded-full bg-blue-600" />
                      )}
                    </div>

                    <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-slate-500">
                      <span>{formatRelativeTime(notification.createdAt)}</span>
                      {notification.sourceEvent && (
                        <span className="rounded-full bg-slate-100 px-2 py-1 font-medium uppercase tracking-wide text-slate-600">
                          {notification.sourceEvent}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
