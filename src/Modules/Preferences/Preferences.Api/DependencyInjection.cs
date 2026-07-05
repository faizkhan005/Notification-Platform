using Microsoft.AspNetCore.Routing;
using Preferences.Api.Endpoints;

namespace Preferences.Api;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder MapPreferencesApi(this IEndpointRouteBuilder app)
    {
        app.MapPreferenceEndpoints();
        return app;
    }
}
