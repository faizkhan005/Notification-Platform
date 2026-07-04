using BuildingBlocks.Application;

namespace Notifications.Application.Commands.ReplayNotification;

public sealed record ReplayNotificationCommand(Guid NotificationId) : ICommand<ReplayNotificationResponse>;

