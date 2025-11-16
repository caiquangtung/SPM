namespace project_service.DTOs;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> CreateFail(string message, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Non-generic API response for simple message responses
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse CreateSuccess(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse CreateFail(string message, string? errorCode = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

