using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure.Persistence;
using Tenants.Infrastructure.Persistence;

namespace NotificationPlatform.IntegrationTests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace TenantsDbContext
            var tenantsDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<TenantsDbContext>));
            if (tenantsDescriptor is not null)
                services.Remove(tenantsDescriptor);

            services.AddDbContext<TenantsDbContext>(options =>
                options.UseNpgsql(_connectionString, npgsql =>
                    npgsql.MigrationsAssembly(
                        typeof(Tenants.Infrastructure.DependencyInjection).Assembly.FullName)));

            // Replace NotificationsDbContext
            var notificationsDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<NotificationsDbContext>));
            if (notificationsDescriptor is not null)
                services.Remove(notificationsDescriptor);

            services.AddDbContext<NotificationsDbContext>(options =>
                options.UseNpgsql(_connectionString, npgsql =>
                    npgsql.MigrationsAssembly(
                        typeof(Notifications.Infrastructure.DependencyInjection).Assembly.FullName)));
        });
    }
}
