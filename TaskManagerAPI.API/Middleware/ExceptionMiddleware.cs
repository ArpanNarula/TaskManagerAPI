using System.Net;
using System.Text.Json;
using TaskManagerAPI.Core.Common;

namespace TaskManagerAPI.API.Middleware;

/// <summary>
/// Global exception middleware — the last safety net before a 500 escapes to the client.
///
/// Why middleware instead of filters?
/// Middleware catches exceptions from ALL layers including routing and model binding,
/// not just from controller action methods.
///
/// Maps known exception types to proper HTTP status codes so controllers
/// don't need try/catch blocks for predictable error scenarios.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            InvalidOperationException   => (HttpStatusCode.BadRequest,          ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,        ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,            ex.Message),
            ArgumentException           => (HttpStatusCode.BadRequest,          ex.Message),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = ApiResponse<object>.Fail(
            message,
            // Include stack trace only in Development to avoid leaking internals
            errors: _env.IsDevelopment() ? new[] { ex.StackTrace ?? string.Empty } : null);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
