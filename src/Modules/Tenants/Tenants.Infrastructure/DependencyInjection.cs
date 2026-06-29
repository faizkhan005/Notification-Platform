using BuildingBlocks.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenants.Application;
using Tenants.Domain;
using Tenants.Infrastructure.Persistence;

namespace Tenants.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TenantsDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("TenantsDb"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName));
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantsUnitOfWork, UnitOfWork>();

        return services;
    }
}
