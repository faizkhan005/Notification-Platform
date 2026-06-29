using BuildingBlocks.Application;

namespace Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    Guid TenantId,
    string Channel,
    string RecipientAddress,
    string? RecipientName,
    string Subject,
    string Body
) : ICommand<SendNotificationResponse>;
