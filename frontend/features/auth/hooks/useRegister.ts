"use client";

import { useMutation } from "@tanstack/react-query";
import { useAuth } from "@/contexts/AuthContext";

export function useRegister() {
  const { register } = useAuth();
  return useMutation({
    mutationFn: async (data: {
      email: string;
      password: string;
      fullName?: string;
    }) => {
      await register(data.email, data.password, data.fullName);
    },
  });
}

export default useRegister;
