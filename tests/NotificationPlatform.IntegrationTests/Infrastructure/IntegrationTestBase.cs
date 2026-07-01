using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure.Persistence;
using Tenants.Infrastructure.Persistence;

namespace NotificationPlatform.IntegrationTests.Infrastructure;

[Collection("IntegrationTests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    protected TestWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory(_fixture.ConnectionString);
        Client = Factory.CreateClient();
        await MigrateDatabasesAsync();
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected T GetService<T>() where T : notnull
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    private async Task MigrateDatabasesAsync()
    {
        using var scope = Factory.Services.CreateScope();

        var tenantsDb = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
        await tenantsDb.Database.MigrateAsync();

        var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await notificationsDb.Database.MigrateAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE tenants RESTART IDENTITY CASCADE");

        var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await notificationsDb.Database.ExecuteSqlRawAsync("TRUNCATE TABLE notifications RESTART IDENTITY CASCADE");
    }
}
