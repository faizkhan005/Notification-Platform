namespace Notifications.Contracts;

public sealed record NotificationCreatedIntegrationEvent(
    Guid NotificationId,
    Guid TenantId,
    string Channel,
    string RecipientAddress,
    DateTimeOffset OccurredAt
);
