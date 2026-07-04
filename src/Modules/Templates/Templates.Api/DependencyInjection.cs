using Microsoft.AspNetCore.Routing;
using Templates.Api.Endpoints;

namespace Templates.Api;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder MapTemplatesApi(this IEndpointRouteBuilder app)
    {
        app.MapTemplateEndpoints();
        return app;
    }
}