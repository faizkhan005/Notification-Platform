using Microsoft.AspNetCore.Routing;
using Tenants.Api.Endpoints;

namespace Tenants.Api;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder MapTenantsApi(this IEndpointRouteBuilder app)
    {
        app.MapTenantEndpoints();
        return app;
    }
}
