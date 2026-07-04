using Notifications.Application;

namespace NotificationPlatform.Api.Infrastructure;

public sealed class HttpContextCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCorrelationId()
    {
        return _httpContextAccessor.HttpContext?.Response.Headers["X-Correlation-Id"].ToString();
    }
}
