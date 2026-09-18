using System.Net;
using System.Text.Json;

namespace RFMAS.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled server exceptions,
/// logs structured diagnostic details, and emits client-friendly JSON error envelopes without exposing internal stack traces.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception encountered while processing HTTP {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorTitle, message) = exception switch
        {
            ArgumentException argEx => (HttpStatusCode.BadRequest, "Invalid Parameter", argEx.Message),
            FormatException formatEx => (HttpStatusCode.BadRequest, "Format Error", formatEx.Message),
            KeyNotFoundException notFoundEx => (HttpStatusCode.NotFound, "Resource Not Found", notFoundEx.Message),
            InvalidOperationException opEx => (HttpStatusCode.Conflict, "Invalid Operation", opEx.Message),
            _ => (HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred while processing your request.")
        };

        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            statusCode = (int)statusCode,
            error = errorTitle,
            message,
            timestampUtc = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }
}
