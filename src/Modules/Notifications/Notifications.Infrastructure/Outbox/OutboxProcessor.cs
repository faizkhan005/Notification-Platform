using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Contracts;
using Notifications.Domain.Events;
using Notifications.Infrastructure.Persistence;
using System.Text.Json;

namespace Notifications.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the outbox table for unpublished messages
/// and publishes them to RabbitMQ via MassTransit.
///
/// This runs INSIDE the API process (registered as a HostedService) for now.
/// In a higher-scale deployment, this would be its own separate process so
/// API request handling and outbox polling don't compete for resources.
/// For this project's scale, running it in-process is a reasonable tradeoff.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // A failure in the polling loop itself (not a single message failure)
                // should not crash the background service. Log and keep polling.
                _logger.LogError(ex, "Outbox processor encountered an unexpected error.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Fetch a batch of unprocessed messages, oldest first.
        // Ordering by OccurredAt preserves rough chronological delivery order.
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                // Deserialize back into the concrete domain event type using the
                // AssemblyQualifiedName we stored when the message was created.
                var eventType = Type.GetType(message.Type)
                    ?? throw new InvalidOperationException($"Unknown event type: {message.Type}");

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType)
                    ?? throw new InvalidOperationException("Failed to deserialize outbox message.");

                // Map the domain event to the integration event contract before publishing.
                // We only know how to map NotificationCreated for now — as more event
                // types get added to the outbox, this switch grows.
                if (domainEvent is NotificationCreated notificationCreated)
                {
                    await publishEndpoint.Publish(new NotificationCreatedIntegrationEvent(
                        notificationCreated.NotificationId.Value,
                        notificationCreated.TenantId,
                        notificationCreated.Channel,
                        notificationCreated.RecipientAddress,
                        notificationCreated.OccurredAt), cancellationToken);
                }

                message.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process outbox message {MessageId} of type {Type}",
                    message.Id, message.Type);

                message.MarkAsFailed(ex.Message);
            }
        }

        // Save all status changes (ProcessedAt or Error/RetryCount) in one batch.
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

