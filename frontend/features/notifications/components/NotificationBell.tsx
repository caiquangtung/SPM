"use client";

import { useEffect, useRef, useState } from "react";
import { Bell } from "lucide-react";
import { useNotificationConnection } from "../hooks/useNotificationConnection";
import {
  useMarkNotificationRead,
  useNotificationStats,
  useNotifications,
} from "../hooks/useNotifications";
import { NotificationDropdown } from "./NotificationDropdown";
import type { Notification } from "@/types/notification";

export function NotificationBell() {
  useNotificationConnection();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement | null>(null);

  const { data: stats } = useNotificationStats();
  const { data: notifications = [] } = useNotifications({ take: 5 });
  const markAsRead = useMarkNotificationRead();

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleEscape);
    };
  }, []);

  const handleMarkRead = (notification: Notification) => {
    if (!notification.isRead) {
      markAsRead.mutate(notification.id);
    }

    setIsOpen(false);
  };

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-label="Open notifications"
        onClick={() => setIsOpen((current) => !current)}
        className="relative flex h-10 w-10 items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-600 transition-colors hover:border-blue-300 hover:text-blue-700 hover:shadow-sm"
      >
        <Bell className="h-5 w-5" />
        {(stats?.unread ?? 0) > 0 && (
          <span className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-blue-600 px-1 text-[11px] font-semibold leading-none text-white shadow-lg shadow-blue-600/30">
            {stats?.unread}
          </span>
        )}
      </button>

      {isOpen && (
        <NotificationDropdown
          notifications={notifications}
          unreadCount={stats?.unread ?? 0}
          onMarkRead={handleMarkRead}
          onClose={() => setIsOpen(false)}
        />
      )}
    </div>
  );
}
