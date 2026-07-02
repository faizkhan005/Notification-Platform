using MassTransit;
using NotificationPlatform.DeliveryWorker.Providers;
using Notifications.Application;
using Notifications.Contracts;
using Notifications.Domain;

namespace NotificationPlatform.DeliveryWorker.Consumers;

// <summary>
/// Consumes NotificationCreatedIntegrationEvent from RabbitMQ and delivers
/// the notification via the appropriate channel provider.
///
/// MassTransit registers this as a consumer for the queue bound to the
/// "Notifications.Contracts:NotificationCreatedIntegrationEvent" exchange
/// we saw created earlier. Once this consumer is registered, MassTransit
/// creates the queue and binds it automatically.
/// </summary>
public sealed class NotificationCreatedConsumer : IConsumer<NotificationCreatedIntegrationEvent>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationsUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationCreatedConsumer> _logger;

    public NotificationCreatedConsumer(
        INotificationRepository repository,
        Notifications.Application.INotificationsUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<NotificationCreatedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        var notificationId = NotificationId.From(message.NotificationId);

        var notification = await _repository.GetByIdAsync(notificationId, context.CancellationToken);

        if (notification is null)
        {
            _logger.LogWarning("Notification {NotificationId} not found — skipping delivery.", message.NotificationId);
            return;
        }

        notification.MarkAsProcessing();
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        try
        {
            // Only Email is implemented right now — other channels get added
            // as their own case here, each with their own provider.
            switch (notification.Channel)
            {
                case NotificationChannel.Email:
                    await _emailSender.SendAsync(
                        notification.Recipient.Address,
                        notification.Content.Subject,
                        notification.Content.Body,
                        context.CancellationToken);
                    break;

                default:
                    throw new NotSupportedException(
                        $"Channel {notification.Channel} is not yet implemented.");
            }

            notification.MarkAsDelivered();
            _logger.LogInformation("Notification {NotificationId} delivered successfully.", message.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver notification {NotificationId}.", message.NotificationId);
            notification.MarkAsFailed(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
