using Cpq.Api.Services.Rules;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cpq.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            EntityNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                ex.Message),

            ConcurrencyConflictException ex => (
                StatusCodes.Status409Conflict,
                "Concurrency Conflict",
                ex.Message),

            RulesEngineException ex => (
                StatusCodes.Status502BadGateway,
                "Rules Engine Error",
                ex.Message),

            InvalidOperationException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                ex.Message),

            ArgumentException ex => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(exception, "Client error {StatusCode}: {Message}", statusCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        var correlationId = httpContext.Items.TryGetValue("CorrelationId", out var cid)
            ? cid?.ToString()
            : null;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (correlationId is not null)
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
