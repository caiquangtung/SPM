import { AxiosResponse } from "axios";
import { ApiResponse, ApiResponseSimple } from "@/types/project";

/**
 * Unwraps ApiResponse<T> to extract the data payload
 * Throws error if response is not successful
 */
export function unwrapResponse<T>(response: AxiosResponse<ApiResponse<T>>): T {
  const apiResponse = response.data;

  if (!apiResponse.success) {
    throw new Error(apiResponse.message || "API request failed");
  }

  if (!apiResponse.data) {
    throw new Error("API response missing data");
  }

  return apiResponse.data;
}

/**
 * Unwraps ApiResponseSimple to extract the message
 * Throws error if response is not successful
 */
export function unwrapSimpleResponse(
  response: AxiosResponse<ApiResponseSimple>
): string {
  const apiResponse = response.data;

  if (!apiResponse.success) {
    throw new Error(apiResponse.message || "API request failed");
  }

  return apiResponse.message;
}

/**
 * Custom error class for API errors
 */
export class ApiError extends Error {
  constructor(
    message: string,
    public errorCode?: string,
    public statusCode?: number
  ) {
    super(message);
    this.name = "ApiError";
  }
}

/**
 * Handles API errors and extracts error information
 */
export function handleApiError(error: any): ApiError {
  if (error.response?.data) {
    const apiResponse = error.response.data as ApiResponseSimple;
    return new ApiError(
      apiResponse.message || "An error occurred",
      apiResponse.errorCode,
      error.response.status
    );
  }

  return new ApiError(error.message || "An unexpected error occurred");
}
