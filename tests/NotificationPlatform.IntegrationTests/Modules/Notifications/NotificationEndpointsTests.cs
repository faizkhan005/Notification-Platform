using FluentAssertions;
using NotificationPlatform.IntegrationTests.Infrastructure;
using Notifications.Application.Commands.SendNotification;
using Notifications.Application.Queries.GetNotificationById;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NotificationPlatform.IntegrationTests.Modules.Notifications;

[Collection("IntegrationTests")]
public sealed class NotificationEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationEndpointsTests(PostgresContainerFixture fixture) : base(fixture) { }

    private async Task<Guid> CreateTenantAsync()
    {
        var request = new { name = "Test Corp", slug = "test-corp", plan = "Pro" };
        var response = await Client.PostAsJsonAsync("/v1/tenants", request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task SendNotification_ValidRequest_Returns202()
    {
        var tenantId = await CreateTenantAsync();
        var request = new
        {
            tenantId,
            channel = "Email",
            recipientAddress = "test@example.com",
            subject = "Hello",
            body = "Test body"
        };

        var response = await Client.PostAsJsonAsync("/v1/notifications", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content
            .ReadFromJsonAsync<SendNotificationResponse>(JsonOptions);
        result!.Status.Should().Be("Pending");
        result.NotificationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SendNotification_InvalidTenant_Returns400()
    {
        var request = new
        {
            tenantId = Guid.NewGuid(),
            channel = "Email",
            recipientAddress = "test@example.com",
            subject = "Hello",
            body = "Test body"
        };

        var response = await Client.PostAsJsonAsync("/v1/notifications", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendNotification_InvalidChannel_Returns400()
    {
        var tenantId = await CreateTenantAsync();
        var request = new
        {
            tenantId,
            channel = "InvalidChannel",
            recipientAddress = "test@example.com",
            subject = "Hello",
            body = "Test body"
        };

        var response = await Client.PostAsJsonAsync("/v1/notifications", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNotificationById_ExistingNotification_Returns200()
    {
        var tenantId = await CreateTenantAsync();
        var sendRequest = new
        {
            tenantId,
            channel = "Email",
            recipientAddress = "test@example.com",
            subject = "Hello",
            body = "Test body"
        };

        var sendResponse = await Client.PostAsJsonAsync("/v1/notifications", sendRequest);
        var sent = await sendResponse.Content
            .ReadFromJsonAsync<SendNotificationResponse>(JsonOptions);

        var response = await Client.GetAsync($"/v1/notifications/{sent!.NotificationId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<NotificationResponse>(JsonOptions);
        result!.Channel.Should().Be("Email");
        result.Status.Should().Be("Pending");
        result.RecipientAddress.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetNotificationById_NonExistent_Returns404()
    {
        var response = await Client.GetAsync($"/v1/notifications/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
