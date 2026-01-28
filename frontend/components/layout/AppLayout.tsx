"use client";

import { ReactNode } from "react";
import Sidebar from "./Sidebar";
import ProtectedRoute from "../common/ProtectedRoute";

interface AppLayoutProps {
  children: ReactNode;
}

export default function AppLayout({ children }: AppLayoutProps) {
  return (
    <ProtectedRoute>
      <div className="flex h-screen bg-gray-50">
        <Sidebar />
        <main className="flex-1 overflow-y-auto">
          <div className="lg:hidden h-16" /> {/* Spacer for mobile header */}
          {children}
        </main>
      </div>
    </ProtectedRoute>
  );
}
