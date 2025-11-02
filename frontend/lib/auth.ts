import Cookies from "js-cookie";
import apiClient from "./api";

export interface User {
  id: string;
  email: string;
  emailConfirmed: boolean;
  fullName?: string;
  avatarUrl?: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export const authService = {
  async register(email: string, password: string, fullName?: string) {
    const response = await apiClient.post<{
      message: string;
      userId: string;
      verificationToken: string;
    }>("/auth/register", { email, password, fullName });
    return response.data;
  },

  async login(email: string, password: string) {
    const response = await apiClient.post<AuthResponse>("/auth/login", {
      email,
      password,
    });

    // Store tokens in cookies
    Cookies.set("access_token", response.data.accessToken, { expires: 1 });
    Cookies.set("refresh_token", response.data.refreshToken, { expires: 7 });

    return response.data;
  },

  async verifyEmail(token: string) {
    const response = await apiClient.post<{ message: string }>(
      "/auth/verify-email",
      { token }
    );
    return response.data;
  },

  async refreshToken() {
    const refreshToken = Cookies.get("refresh_token");
    if (!refreshToken) {
      throw new Error("No refresh token");
    }

    const response = await apiClient.post<AuthResponse>("/auth/refresh", {
      refreshToken,
    });

    Cookies.set("access_token", response.data.accessToken, { expires: 1 });
    Cookies.set("refresh_token", response.data.refreshToken, { expires: 7 });

    return response.data;
  },

  logout() {
    Cookies.remove("access_token");
    Cookies.remove("refresh_token");
    window.location.href = "/login";
  },

  getAccessToken(): string | undefined {
    return Cookies.get("access_token");
  },

  getRefreshToken(): string | undefined {
    return Cookies.get("refresh_token");
  },
};

export async function getServerSession(): Promise<User | null> {
  // This would typically validate the token server-side
  // For now, we'll return null and handle auth client-side
  return null;
}
