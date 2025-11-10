"use client";

import { useMutation } from "@tanstack/react-query";
import { useAuth } from "@/contexts/AuthContext";

export function useLogin() {
  const { login } = useAuth();
  return useMutation({
    mutationFn: async (data: { email: string; password: string }) => {
      await login(data.email, data.password);
    },
  });
}

export default useLogin;
