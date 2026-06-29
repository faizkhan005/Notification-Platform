namespace Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationResponse(
    Guid NotificationId,
    string Status,
    DateTimeOffset CreatedAt
);
