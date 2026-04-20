namespace notification_service.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
        => new() { Success = true, Message = message, Data = data, Timestamp = DateTime.UtcNow };

    public static ApiResponse<T> CreateFail(string message, string? errorCode = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode, Timestamp = DateTime.UtcNow };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse CreateSuccess(string message = "Operation successful")
        => new() { Success = true, Message = message, Timestamp = DateTime.UtcNow };

    public static ApiResponse CreateFail(string message, string? errorCode = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode, Timestamp = DateTime.UtcNow };
}