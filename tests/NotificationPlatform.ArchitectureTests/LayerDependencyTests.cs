using NetArchTest.Rules;
using System.Reflection;

namespace NotificationPlatform.ArchitectureTests;

public sealed class LayerDependencyTests
{
    // Tenants
    private static readonly Assembly TenantsDomain =
        typeof(Tenants.Domain.Tenant).Assembly;
    private static readonly Assembly TenantsApplication =
        typeof(Tenants.Application.Commands.CreateTenant.CreateTenantCommand).Assembly;
    private static readonly Assembly TenantsInfrastructure =
        typeof(Tenants.Infrastructure.Persistence.TenantsDbContext).Assembly;
    private static readonly Assembly TenantsApi =
        typeof(Tenants.Api.Endpoints.TenantEndpoints).Assembly;

    // Notifications
    private static readonly Assembly NotificationsDomain =
        typeof(Notifications.Domain.Notification).Assembly;
    private static readonly Assembly NotificationsApplication =
        typeof(Notifications.Application.Commands.SendNotification.SendNotificationCommand).Assembly;
    private static readonly Assembly NotificationsInfrastructure =
        typeof(Notifications.Infrastructure.Persistence.NotificationsDbContext).Assembly;
    private static readonly Assembly NotificationsApi =
        typeof(Notifications.Api.Endpoints.NotificationEndpoints).Assembly;

    // TENANTS LAYER RULES

    [Fact]
    public void TenantsDomain_ShouldNotReference_TenantsApplication()
    {
        var result = Types.InAssembly(TenantsDomain)
            .ShouldNot().HaveDependencyOn(TenantsApplication.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsDomain_ShouldNotReference_TenantsInfrastructure()
    {
        var result = Types.InAssembly(TenantsDomain)
            .ShouldNot().HaveDependencyOn(TenantsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsApplication_ShouldNotReference_TenantsInfrastructure()
    {
        var result = Types.InAssembly(TenantsApplication)
            .ShouldNot().HaveDependencyOn(TenantsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsApi_ShouldNotReference_TenantsInfrastructure()
    {
        var result = Types.InAssembly(TenantsApi)
            .ShouldNot().HaveDependencyOn(TenantsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsApi_ShouldNotReference_TenantsDomain()
    {
        var result = Types.InAssembly(TenantsApi)
            .ShouldNot().HaveDependencyOn(TenantsDomain.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    // NOTIFICATIONS LAYER RULES

    [Fact]
    public void NotificationsDomain_ShouldNotReference_NotificationsApplication()
    {
        var result = Types.InAssembly(NotificationsDomain)
            .ShouldNot().HaveDependencyOn(NotificationsApplication.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void NotificationsDomain_ShouldNotReference_NotificationsInfrastructure()
    {
        var result = Types.InAssembly(NotificationsDomain)
            .ShouldNot().HaveDependencyOn(NotificationsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void NotificationsApplication_ShouldNotReference_NotificationsInfrastructure()
    {
        var result = Types.InAssembly(NotificationsApplication)
            .ShouldNot().HaveDependencyOn(NotificationsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void NotificationsApi_ShouldNotReference_NotificationsInfrastructure()
    {
        var result = Types.InAssembly(NotificationsApi)
            .ShouldNot().HaveDependencyOn(NotificationsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void NotificationsApi_ShouldNotReference_NotificationsDomain()
    {
        var result = Types.InAssembly(NotificationsApi)
            .ShouldNot().HaveDependencyOn(NotificationsDomain.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    // HELPER

    private static string Format(TestResult result)
    {
        var types = result.FailingTypes?
            .Select(t => $"  - {t.FullName}") ?? [];
        return $"Violating types:\n{string.Join("\n", types)}";
    }
}
