namespace Notifications.Application.Commands.ReplayNotification;

public sealed record ReplayNotificationResponse(
    Guid NotificationId,
    string Status
);
