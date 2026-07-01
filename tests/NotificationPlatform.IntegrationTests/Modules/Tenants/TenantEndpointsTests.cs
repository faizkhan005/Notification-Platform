using FluentAssertions;
using NotificationPlatform.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tenants.Application.Commands.CreateTenant;

namespace NotificationPlatform.IntegrationTests.Modules.Tenants;

[Collection("IntegrationTests")]
public sealed class TenantEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TenantEndpointsTests(PostgresContainerFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateTenant_WithValidRequest_Returns201()
    {
        var request = new { name = "Acme Corp", slug = "acme-corp", plan = "Pro" };

        var response = await Client.PostAsJsonAsync("/v1/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateTenantResponse>(JsonOptions);
        body!.Name.Should().Be("Acme Corp");
        body.Slug.Should().Be("acme-corp");
        body.Plan.Should().Be("Pro");
    }

    [Fact]
    public async Task CreateTenant_DuplicateSlug_Returns409()
    {
        var request = new { name = "Acme Corp", slug = "acme-corp", plan = "Pro" };
        await Client.PostAsJsonAsync("/v1/tenants", request);

        var response = await Client.PostAsJsonAsync("/v1/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateTenant_EmptyName_Returns400()
    {
        var request = new { name = "", slug = "acme-corp", plan = "Pro" };

        var response = await Client.PostAsJsonAsync("/v1/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTenantBySlug_ExistingTenant_Returns200()
    {
        var request = new { name = "Acme Corp", slug = "acme-corp", plan = "Pro" };
        await Client.PostAsJsonAsync("/v1/tenants", request);

        var response = await Client.GetAsync("/v1/tenants/acme-corp");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTenantBySlug_NonExistent_Returns404()
    {
        var response = await Client.GetAsync("/v1/tenants/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
