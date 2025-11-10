import { z } from "zod";

export const registerSchema = z
  .object({
    email: z.string().trim().email({ message: "Email không hợp lệ" }),
    password: z
      .string()
      .min(6, { message: "Mật khẩu tối thiểu 6 ký tự" })
      .max(128, { message: "Mật khẩu quá dài" }),
    confirmPassword: z
      .string()
      .min(1, { message: "Vui lòng xác nhận mật khẩu" }),
    fullName: z
      .string()
      .trim()
      .max(100, { message: "Họ tên quá dài" })
      .optional()
      .or(z.literal("")),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Mật khẩu xác nhận không khớp",
    path: ["confirmPassword"],
  });

export type RegisterForm = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.string().trim().email({ message: "Email không hợp lệ" }),
  password: z.string().min(1, { message: "Vui lòng nhập mật khẩu" }),
});

export type LoginForm = z.infer<typeof loginSchema>;

export const verifyEmailSchema = z.object({
  token: z.string().min(1, { message: "Token không hợp lệ" }),
});

export type VerifyEmailForm = z.infer<typeof verifyEmailSchema>;
