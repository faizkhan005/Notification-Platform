using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("NotificationsDb"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName));
        });

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationsUnitOfWork, UnitOfWork>();

        return services;
    }
}
