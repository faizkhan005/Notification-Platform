using NetArchTest.Rules;
using System.Reflection;

namespace NotificationPlatform.ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    private static readonly Assembly TenantsDomain =
        typeof(Tenants.Domain.Tenant).Assembly;
    private static readonly Assembly TenantsApplication =
        typeof(Tenants.Application.Commands.CreateTenant.CreateTenantCommand).Assembly;
    private static readonly Assembly TenantsInfrastructure =
        typeof(Tenants.Infrastructure.Persistence.TenantsDbContext).Assembly;

    private static readonly Assembly NotificationsDomain =
        typeof(Notifications.Domain.Notification).Assembly;
    private static readonly Assembly NotificationsInfrastructure =
        typeof(Notifications.Infrastructure.Persistence.NotificationsDbContext).Assembly;

    [Fact]
    public void TenantsDomain_ShouldNotReference_NotificationsModule()
    {
        var result = Types.InAssembly(TenantsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(
                NotificationsDomain.GetName().Name,
                NotificationsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsApplication_ShouldNotReference_NotificationsModule()
    {
        var result = Types.InAssembly(TenantsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(
                NotificationsDomain.GetName().Name,
                NotificationsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void TenantsInfrastructure_ShouldNotReference_NotificationsDomain()
    {
        var result = Types.InAssembly(TenantsInfrastructure)
            .ShouldNot()
            .HaveDependencyOn(NotificationsDomain.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void NotificationsDomain_ShouldNotReference_TenantsInfrastructure()
    {
        var result = Types.InAssembly(NotificationsDomain)
            .ShouldNot()
            .HaveDependencyOn(TenantsInfrastructure.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    private static string Format(TestResult result)
    {
        var types = result.FailingTypes?
            .Select(t => $"  - {t.FullName}") ?? [];
        return $"Violating types:\n{string.Join("\n", types)}";
    }
}
