"use client";

import { AppLayout } from "@/components/layout";
import { useAuth } from "@/contexts/AuthContext";
import Link from "next/link";
import { FolderKanban, CheckSquare, Users, TrendingUp } from "lucide-react";

export default function DashboardPage() {
  const { user } = useAuth();

  const stats = [
    {
      name: "Total Projects",
      value: "12",
      icon: FolderKanban,
      change: "+2 this month",
      changeType: "increase",
    },
    {
      name: "Active Tasks",
      value: "34",
      icon: CheckSquare,
      change: "8 completed",
      changeType: "neutral",
    },
    {
      name: "Team Members",
      value: "8",
      icon: Users,
      change: "+1 this week",
      changeType: "increase",
    },
    {
      name: "Completion Rate",
      value: "87%",
      icon: TrendingUp,
      change: "+5% from last week",
      changeType: "increase",
    },
  ];

  return (
    <AppLayout>
      <div className="p-6 sm:p-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-gray-600">
            Welcome back, {user?.email || "User"}!
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {stats.map((stat) => {
            const Icon = stat.icon;
            return (
              <div
                key={stat.name}
                className="bg-white rounded-lg shadow-sm border border-gray-200 p-6"
              >
                <div className="flex items-center justify-between mb-4">
                  <div className="p-2 bg-blue-50 rounded-lg">
                    <Icon className="w-6 h-6 text-blue-600" />
                  </div>
                </div>
                <div className="text-2xl font-bold text-gray-900 mb-1">
                  {stat.value}
                </div>
                <div className="text-sm text-gray-600 mb-2">{stat.name}</div>
                <div
                  className={`text-xs ${
                    stat.changeType === "increase"
                      ? "text-green-600"
                      : "text-gray-500"
                  }`}
                >
                  {stat.change}
                </div>
              </div>
            );
          })}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Projects Card */}
          <div className="bg-white shadow-sm rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900">
                Recent Projects
              </h2>
              <Link
                href="/projects"
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                View all →
              </Link>
            </div>
            <p className="text-gray-600 mb-4">
              Manage your projects and track progress
            </p>
            <Link
              href="/projects"
              className="inline-flex items-center justify-center w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <FolderKanban className="w-5 h-5 mr-2" />
              Go to Projects
            </Link>
          </div>

          {/* Tasks Card */}
          <div className="bg-white shadow-sm rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900">
                My Tasks
              </h2>
              <Link
                href="/tasks"
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                View all →
              </Link>
            </div>
            <p className="text-gray-600 mb-4">
              Track and manage your assigned tasks
            </p>
            <Link
              href="/tasks"
              className="inline-flex items-center justify-center w-full px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <CheckSquare className="w-5 h-5 mr-2" />
              View Tasks
            </Link>
          </div>
        </div>
      </div>
    </AppLayout>
  );
}
