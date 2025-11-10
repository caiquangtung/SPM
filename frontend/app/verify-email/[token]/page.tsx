"use client";

import { useParams } from "next/navigation";
import { VerifyEmailView } from "@/features/auth/components";

export default function VerifyEmailPage() {
  const params = useParams();
  const token = (params?.token as string) || "";
  return <VerifyEmailView token={token} />;
}
