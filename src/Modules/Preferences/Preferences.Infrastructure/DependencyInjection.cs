using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Preferences.Application;
using Preferences.Domain;
using Preferences.Infrastructure.Persistence;

namespace Preferences.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPreferencesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PreferencesDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("PreferencesDb"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName));
        });

        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IPreferencesUnitOfWork, Persistence.UnitOfWork>();

        return services;
    }
}
