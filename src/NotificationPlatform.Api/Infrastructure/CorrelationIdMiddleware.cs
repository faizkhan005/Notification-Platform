using Serilog.Context;

namespace NotificationPlatform.Api.Infrastructure;

/// <summary>
/// Ensures every HTTP request has a correlation ID, either provided by the
/// caller via X-Correlation-Id header, or generated fresh if absent.
///
/// The ID is pushed onto Serilog's LogContext, so every log line written
/// during this request automatically includes it — no need to manually
/// pass it into every logger call.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        // Echo it back so the caller can correlate their own logs too.
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
