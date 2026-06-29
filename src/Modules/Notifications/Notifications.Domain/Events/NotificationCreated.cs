using BuildingBlocks.Domain;

namespace Notifications.Domain.Events;

public sealed record NotificationCreated : IDomainEvent
{
    public required NotificationId NotificationId { get; init; }
    public required Guid TenantId { get; init; }
    public required string Channel { get; init; }
    public required string RecipientAddress { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
