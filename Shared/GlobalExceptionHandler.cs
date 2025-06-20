using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Shared;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger): IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode = StatusCodes.Status500InternalServerError;
        if (exception is AppException)
        {
            statusCode = StatusCodes.Status400BadRequest;
        }
        
        ProblemDetails? problemDetails = new ()
        {
            Status = statusCode,
            Title = "Error",
            Detail = exception?.Message ?? "",
            Extensions = new Dictionary<string, object?>
            {
                { "innerException", exception?.InnerException?.Message ?? "N/A" },
                { "path", httpContext.Request.Path.Value },
                { "method", httpContext.Request.Method },
                { "datetime", DateTime.UtcNow  }
            }
        };
        
        string? extensionsJson = JsonSerializer.Serialize(problemDetails);

        if (exception is not AppException)
        {
            logger.LogError(exception, "Error = {error}", extensionsJson);
        }
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}