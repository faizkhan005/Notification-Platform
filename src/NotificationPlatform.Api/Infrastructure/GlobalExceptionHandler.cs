using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tenants.Domain.Exceptions;

namespace NotificationPlatform.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
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
        _logger.LogError(exception, "Unhandled exception {TraceId}", httpContext.TraceIdentifier);

        var (statusCode, problem) = exception switch
        {
            ValidationException ex => (StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = httpContext.Request.Path
                }),

            TenantDomainException ex when ex.Message.Contains("already taken") => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Instance = httpContext.Request.Path
                }),

            TenantDomainException ex => (StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Domain Rule Violation",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = httpContext.Request.Path
                }),

            _ => (StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = httpContext.Request.Path,
                    Extensions = { ["traceId"] = httpContext.TraceIdentifier }
                })
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

