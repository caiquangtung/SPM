"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { authService } from "@/lib/auth";
import { toast } from "sonner";
import { verifyEmailSchema } from "@/lib/validators/auth";

export default function VerifyEmailView({ token }: { token: string }) {
  const router = useRouter();
  const [loading, setLoading] = useState(true);
  const [verified, setVerified] = useState(false);

  useEffect(() => {
    const parsed = verifyEmailSchema.safeParse({ token });
    if (!parsed.success) {
      toast.error("Invalid verification token");
      router.push("/login");
      return;
    }

    const verifyEmail = async () => {
      try {
        await authService.verifyEmail(parsed.data.token);
        setVerified(true);
        toast.success("Email verified successfully!");
        setTimeout(() => {
          router.push("/login");
        }, 2000);
      } catch (error: any) {
        toast.error(
          error?.response?.data?.message || "Email verification failed"
        );
        setTimeout(() => {
          router.push("/login");
        }, 2000);
      } finally {
        setLoading(false);
      }
    };

    verifyEmail();
  }, [token, router]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full bg-white p-8 rounded-lg shadow-md text-center">
        {verified ? (
          <>
            <div className="text-green-500 text-6xl mb-4">✓</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              Email Verified!
            </h2>
            <p className="text-gray-600">
              Your email has been successfully verified.
            </p>
            <p className="text-gray-500 text-sm mt-2">
              Redirecting to login...
            </p>
          </>
        ) : (
          <>
            <div className="text-red-500 text-6xl mb-4">✗</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              Verification Failed
            </h2>
            <p className="text-gray-600">
              The verification link is invalid or has expired.
            </p>
            <p className="text-gray-500 text-sm mt-2">
              Redirecting to login...
            </p>
          </>
        )}
      </div>
    </div>
  );
}
