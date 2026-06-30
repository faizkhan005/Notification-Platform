using Microsoft.AspNetCore.Routing;
using Notifications.Api.Endpoints;

namespace Notifications.Api;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder MapNotificationsApi(this IEndpointRouteBuilder app)
    {
        app.MapNotificationEndpoints();
        return app;
    }
}
