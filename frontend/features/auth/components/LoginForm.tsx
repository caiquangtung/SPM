"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  loginSchema,
  type LoginForm as LoginFormType,
} from "@/lib/validators/auth";
import { toast } from "sonner";
import Link from "next/link";
import { ControlledInput } from "@/components/forms/ControlledInput";
import { useLogin } from "@/features/auth/hooks/useLogin";

export default function LoginForm() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [remember, setRemember] = useState(false);

  const { control, handleSubmit } = useForm<LoginFormType>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });
  const loginMutation = useLogin();

  const onSubmit = async (data: LoginFormType) => {
    setLoading(true);
    try {
      await loginMutation.mutateAsync({
        email: data.email,
        password: data.password,
      });
      toast.success("Đăng nhập thành công!");
      router.push("/dashboard");
    } catch (error: any) {
      toast.error(error?.response?.data?.message || "Đăng nhập thất bại");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-lg p-8 sm:p-10">
        <div className="flex flex-col items-center">
          <div className="w-16 h-16 rounded-full bg-primary-600 flex items-center justify-center text-white text-2xl font-bold">
            S
          </div>
          <h2 className="mt-4 text-center text-2xl font-extrabold text-gray-900">
            Welcome back
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Sign in to continue to your SPM dashboard.
          </p>
        </div>

        <form className="mt-8" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4">
            <ControlledInput<LoginFormType>
              control={control}
              name="email"
              type="email"
              label="Email"
              placeholder="you@example.com"
              autoComplete="email"
            />

            <Controller
              control={control}
              name="password"
              render={({ field, fieldState }) => (
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Password
                  </label>
                  <div className="mt-1 relative">
                    <input
                      {...field}
                      type={showPassword ? "text" : "password"}
                      placeholder="••••••••"
                      autoComplete="current-password"
                      className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
                    />

                    <button
                      type="button"
                      onClick={() => setShowPassword((s) => !s)}
                      className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700 bg-white rounded-full p-1 focus:outline-none focus:ring-2 focus:ring-primary-500"
                      aria-label={
                        showPassword ? "Hide password" : "Show password"
                      }
                    >
                      {showPassword ? (
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          className="w-5 h-5"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth={2}
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          aria-hidden="true"
                        >
                          <path d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.542-7a9.965 9.965 0 012.11-3.47" />
                          <path d="M3 3l18 18" />
                          <path d="M9.88 9.88a3 3 0 104.24 4.24" />
                        </svg>
                      ) : (
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          className="w-5 h-5"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth={2}
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          aria-hidden="true"
                        >
                          <path d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          <path d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                      )}
                    </button>
                  </div>
                  {fieldState.error && (
                    <p className="mt-1 text-sm text-red-600">
                      {fieldState.error.message}
                    </p>
                  )}
                </div>
              )}
            />

            <div className="flex items-center justify-between">
              <label className="inline-flex items-center text-sm text-gray-700">
                <input
                  type="checkbox"
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                  checked={remember}
                  onChange={(e) => setRemember(e.target.checked)}
                />
                <span className="ml-2">Remember me</span>
              </label>
              <Link
                href="/forgot-password"
                className="text-sm text-primary-600 hover:text-primary-500"
              >
                Forgot password?
              </Link>
            </div>
          </div>

          <div className="mt-6">
            <button
              type="submit"
              disabled={loading}
              className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? "Signing in..." : "Sign in"}
            </button>
          </div>

          <div className="mt-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-200" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">
                  Or continue with
                </span>
              </div>
            </div>

            <div className="mt-4">
              <button
                type="button"
                onClick={() => toast("Social sign-in not implemented")}
                className="w-full inline-flex justify-center items-center py-2 px-4 border rounded-md shadow-sm bg-white text-sm text-gray-700 hover:bg-gray-50"
                aria-label="Sign in with Google"
              >
                <svg
                  className="w-5 h-5 mr-2"
                  viewBox="0 0 24 24"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    d="M21.35 11.1h-9.2v2.8h5.3c-.23 1.34-1.22 2.48-2.6 3.08v2.56h4.2c2.46-2.27 3.88-5.6 3.88-9.44 0-.64-.06-1.26-.13-1.86z"
                    fill="#4285F4"
                  />
                  <path
                    d="M12.15 22c2.7 0 4.97-.9 6.63-2.44l-3.17-2.56c-.88.6-2.02.96-3.46.96-2.66 0-4.91-1.8-5.72-4.23H3.92v2.66C5.58 19.9 8.66 22 12.15 22z"
                    fill="#34A853"
                  />
                  <path
                    d="M6.43 13.73A6.97 6.97 0 016 12c0-.66.1-1.3.28-1.9V7.44H3.92A10 10 0 002 12c0 1.6.36 3.12 1.01 4.46l2.4-2.73z"
                    fill="#FBBC05"
                  />
                  <path
                    d="M12.15 6.4c1.47 0 2.79.51 3.82 1.5l2.85-2.86C16.98 2.99 14.83 2 12.15 2 8.66 2 5.58 4.1 3.92 7.44l2.36 2.66c.81-2.43 3.06-4.2 5.87-4.2z"
                    fill="#EA4335"
                  />
                </svg>
                Sign in with Google
              </button>
            </div>
          </div>

          <p className="mt-6 text-center text-sm text-gray-600">
            Or{" "}
            <Link
              href="/register"
              className="font-medium text-primary-600 hover:text-primary-500"
            >
              create a new account
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
