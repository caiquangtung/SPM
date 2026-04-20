"use client";

import AppLayout from "@/components/layout/AppLayout";
import { NotificationList } from "@/features/notifications/components/NotificationList";

export default function NotificationsPage() {
  return (
    <AppLayout>
      <div className="min-h-full bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.14),_transparent_35%),linear-gradient(180deg,_#f8fafc_0%,_#ffffff_100%)] p-4 sm:p-6 lg:p-8">
        <div className="mx-auto max-w-5xl">
          <NotificationList />
        </div>
      </div>
    </AppLayout>
  );
}
