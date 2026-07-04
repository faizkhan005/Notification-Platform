using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Templates.Application;
using Templates.Domain;
using Templates.Infrastructure.Persistence;

namespace Templates.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTemplatesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TemplatesDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("TemplatesDb"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName));
        });

        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ITemplatesUnitOfWork, Persistence.UnitOfWork>();

        return services;
    }
}
