"use client";

import Link from "next/link";
import { formatDistanceToNow } from "date-fns";
import { ArrowRight, CheckCheck, CircleDot } from "lucide-react";
import type { Notification } from "@/types/notification";

interface NotificationDropdownProps {
  notifications: Notification[];
  unreadCount: number;
  onMarkRead: (notification: Notification) => void;
  onClose: () => void;
}

function formatRelativeTime(isoDate: string) {
  return formatDistanceToNow(new Date(isoDate), { addSuffix: true });
}

export function NotificationDropdown({
  notifications,
  unreadCount,
  onMarkRead,
  onClose,
}: NotificationDropdownProps) {
  return (
    <div className="absolute right-0 top-12 z-50 w-96 max-w-[calc(100vw-2rem)] rounded-2xl border border-slate-200 bg-white shadow-2xl shadow-slate-900/10 overflow-hidden">
      <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
        <div>
          <p className="text-sm font-semibold text-slate-900">Notifications</p>
          <p className="text-xs text-slate-500">
            {unreadCount} unread notification{unreadCount === 1 ? "" : "s"}
          </p>
        </div>
        <button
          onClick={onClose}
          className="text-xs font-medium text-slate-500 hover:text-slate-900"
        >
          Close
        </button>
      </div>

      <div className="max-h-96 overflow-y-auto">
        {notifications.length === 0 ? (
          <div className="px-4 py-8 text-center">
            <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-500">
              <CircleDot className="h-5 w-5" />
            </div>
            <p className="text-sm font-medium text-slate-900">
              No notifications yet
            </p>
            <p className="mt-1 text-xs text-slate-500">
              Notifications will appear here when project activity happens.
            </p>
          </div>
        ) : (
          <div className="divide-y divide-slate-100">
            {notifications.map((notification) => (
              <button
                key={notification.id}
                onClick={() => onMarkRead(notification)}
                className={`w-full text-left px-4 py-3 transition-colors hover:bg-slate-50 ${
                  notification.isRead ? "bg-white" : "bg-blue-50/50"
                }`}
              >
                <div className="flex items-start gap-3">
                  <div
                    className={`mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full ${
                      notification.isRead
                        ? "bg-slate-100 text-slate-500"
                        : "bg-blue-100 text-blue-700"
                    }`}
                  >
                    <CheckCheck className="h-4 w-4" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-sm font-semibold text-slate-900">
                          {notification.title}
                        </p>
                        <p className="mt-1 text-sm text-slate-600 line-clamp-2">
                          {notification.message}
                        </p>
                      </div>
                      {!notification.isRead && (
                        <span className="mt-1 h-2.5 w-2.5 rounded-full bg-blue-600" />
                      )}
                    </div>
                    <div className="mt-2 text-xs text-slate-500">
                      {formatRelativeTime(notification.createdAt)}
                    </div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="border-t border-slate-100 bg-slate-50 px-4 py-3">
        <Link
          href="/notifications"
          onClick={onClose}
          className="inline-flex items-center gap-1 text-sm font-medium text-blue-700 hover:text-blue-800"
        >
          View all notifications
          <ArrowRight className="h-4 w-4" />
        </Link>
      </div>
    </div>
  );
}
