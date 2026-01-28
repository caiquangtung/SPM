import Cookies from "js-cookie";
import apiClient from "./axios";
import { unwrapResponse } from "./api-helpers";
import type { User, AuthResponse } from "@/types/auth";
import type { ApiResponse } from "@/types/project";

export const authService = {
  async register(email: string, password: string, fullName?: string) {
    const response = await apiClient.post<
      ApiResponse<{
        userId: string;
      }>
    >("/auth/register", { email, password, fullName });
    return unwrapResponse(response);
  },

  async login(email: string, password: string) {
    try {
      const response = await apiClient.post<ApiResponse<AuthResponse>>(
        "/auth/login",
        {
          email,
          password,
        }
      );

      // Unwrap ApiResponse<T>
      const authData = unwrapResponse(response);

      // Handle both camelCase and PascalCase (defensive)
      const accessToken = authData.accessToken || (authData as any).AccessToken;
      const refreshToken =
        authData.refreshToken || (authData as any).RefreshToken;
      const user = authData.user || (authData as any).User;

      if (!accessToken || !refreshToken) {
        throw new Error("Invalid login response: missing tokens");
      }

      // Store tokens in cookies
      Cookies.set("access_token", accessToken, { expires: 1 });
      Cookies.set("refresh_token", refreshToken, { expires: 7 });

      return {
        accessToken,
        refreshToken,
        expiresAt: authData.expiresAt || (authData as any).ExpiresAt,
        user: user || authData.user,
      };
    } catch (error) {
      throw error;
    }
  },

  async verifyEmail(token: string) {
    const response = await apiClient.post<ApiResponse<string>>(
      "/auth/verify-email",
      { token }
    );
    return unwrapResponse(response);
  },

  async refreshToken() {
    const refreshToken = Cookies.get("refresh_token");
    if (!refreshToken) {
      throw new Error("No refresh token");
    }

    const response = await apiClient.post<ApiResponse<AuthResponse>>(
      "/auth/refresh",
      {
        refreshToken,
      }
    );

    // Unwrap ApiResponse<T>
    const authData = unwrapResponse(response);

    // Handle both camelCase and PascalCase (defensive)
    const accessToken = authData.accessToken || (authData as any).AccessToken;
    const refreshTokenValue =
      authData.refreshToken || (authData as any).RefreshToken;
    const user = authData.user || (authData as any).User;

    if (!accessToken || !refreshTokenValue) {
      throw new Error("Invalid refresh response: missing tokens");
    }

    Cookies.set("access_token", accessToken, { expires: 1 });
    Cookies.set("refresh_token", refreshTokenValue, { expires: 7 });

    return {
      accessToken,
      refreshToken: refreshTokenValue,
      expiresAt: authData.expiresAt || (authData as any).ExpiresAt,
      user: user || authData.user,
    };
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

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<ApiResponse<User>>("/auth/me");
    return unwrapResponse(response);
  },
};

export async function getServerSession(): Promise<User | null> {
  // This would typically validate the token server-side
  // For now, we'll return null and handle auth client-side
  return null;
}
