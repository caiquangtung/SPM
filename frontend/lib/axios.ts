import axios from "axios";
import Cookies from "js-cookie";

// Get API URL from environment or use default (API Gateway)
const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5010";

const apiClient = axios.create({
  baseURL: `${API_URL}/api`,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add auth token to requests
apiClient.interceptors.request.use((config) => {
  const token = Cookies.get("access_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token refresh on 401
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as any;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = Cookies.get("refresh_token");
        if (!refreshToken) {
          throw new Error("No refresh token");
        }

        const response = await axios.post<{
          success: boolean;
          message: string;
          data?: { accessToken: string; refreshToken: string };
        }>(`${API_URL}/api/auth/refresh`, {
          refreshToken,
        });

        // Unwrap ApiResponse<T>
        const authData = response.data.data;
        if (!authData) {
          throw new Error("Invalid refresh response");
        }

        Cookies.set("access_token", authData.accessToken, { expires: 1 });
        if (authData.refreshToken) {
          Cookies.set("refresh_token", authData.refreshToken, { expires: 7 });
        }

        originalRequest.headers.Authorization = `Bearer ${authData.accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        Cookies.remove("access_token");
        Cookies.remove("refresh_token");
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
