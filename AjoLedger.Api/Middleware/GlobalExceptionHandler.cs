
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unexpected error occurred while processing the request.");

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "The server could not complete your request."
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
