namespace Notifications.Application.Queries.GetNotificationById;

public sealed record NotificationResponse(
    Guid Id,
    Guid TenantId,
    string Channel,
    string RecipientAddress,
    string? RecipientName,
    string Subject,
    string Status,
    string? FailureReason,
    int RetryCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
