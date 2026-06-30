using Microsoft.AspNetCore.Routing;

namespace Notifications.Api.Endpoints;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder MapNotificationsApi(this IEndpointRouteBuilder app)
    {
        app.MapNotificationEndpoints();
        return app;
    }
}
