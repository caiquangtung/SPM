"use client";

import { ProtectedRoute } from "@/components/common";
import { useAuth } from "@/contexts/AuthContext";

export default function DashboardPage() {
  const { user } = useAuth();

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-gray-600">
            Welcome back, {user?.fullName || user?.email}!
          </p>

          <div className="mt-8">
            <div className="bg-white shadow rounded-lg p-6">
              <h2 className="text-xl font-semibold mb-4">Projects</h2>
              <p className="text-gray-600 mb-4">
                Manage your projects and tasks.
              </p>
              <a
                href="/projects"
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                View Projects
              </a>
            </div>
          </div>
        </div>
      </div>
    </ProtectedRoute>
  );
}
