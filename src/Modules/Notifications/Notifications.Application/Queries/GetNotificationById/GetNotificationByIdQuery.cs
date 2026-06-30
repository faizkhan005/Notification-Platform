using BuildingBlocks.Application;

namespace Notifications.Application.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid Id) : IQuery<NotificationResponse?>;

