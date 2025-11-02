using System.Net;
using System.Text.Json;
using user_service.DTOs;

namespace user_service.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        ApiResponse<string> errorResponse;
        string errorCode = "INTERNAL_ERROR";

        switch (exception)
        {
            case InvalidOperationException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                errorResponse = ApiResponse<string>.CreateFail(
                    exception.Message,
                    errorCode
                );
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                errorResponse = ApiResponse<string>.CreateFail(
                    exception.Message,
                    errorCode
                );
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                errorResponse = ApiResponse<string>.CreateFail(
                    exception.Message,
                    errorCode
                );
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = ApiResponse<string>.CreateFail(
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An error occurred while processing your request.",
                    errorCode
                );
                break;
        }

        // Add stack trace in development mode
        if (_environment.IsDevelopment() && !string.IsNullOrEmpty(exception.StackTrace))
        {
            errorResponse.Data = exception.StackTrace;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }
}

