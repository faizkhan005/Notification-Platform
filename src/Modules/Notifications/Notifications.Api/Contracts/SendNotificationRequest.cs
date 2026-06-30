namespace Notifications.Api.Contracts;

public sealed record SendNotificationRequest(
    Guid TenantId,
    string Channel,
    string RecipientAddress,
    string? RecipientName,
    string Subject,
    string Body
);
