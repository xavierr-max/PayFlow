using System.Net;
using System.Text.Json;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { message = exception.Message, error = exception.GetType().Name };

        return exception switch
        {
            NotFoundException => (context.Response.StatusCode = StatusCodes.Status404NotFound, Task.CompletedTask).Item2,
            ValidationException => (context.Response.StatusCode = StatusCodes.Status400BadRequest, Task.CompletedTask).Item2,
            UnauthorizedException => (context.Response.StatusCode = StatusCodes.Status401Unauthorized, Task.CompletedTask).Item2,
            _ => (context.Response.StatusCode = StatusCodes.Status500InternalServerError, Task.CompletedTask).Item2
        };

        #pragma warning disable CS0162
        return context.Response.WriteAsJsonAsync(response);
        #pragma warning restore CS0162
    }
}
