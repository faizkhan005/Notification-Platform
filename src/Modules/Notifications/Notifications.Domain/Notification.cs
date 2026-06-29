using BuildingBlocks.Domain;
using Notifications.Domain.Events;
using Notifications.Domain.Exceptions;
using Tenants.Domain;

namespace Notifications.Domain;

public sealed class Notification
{
    public NotificationId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public Recipient Recipient { get; private set; }
    public NotificationContent Content { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Notification()
    {
        Recipient = null!;
        Content = null!;
    }

    public static Notification Create(
        TenantId tenantId,
        NotificationChannel channel,
        Recipient recipient,
        NotificationContent content)
    {
        var now = DateTimeOffset.UtcNow;

        var notification = new Notification
        {
            Id = NotificationId.New(),
            TenantId = tenantId,
            Channel = channel,
            Recipient = recipient,
            Content = content,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        notification._domainEvents.Add(new NotificationCreated
        {
            NotificationId = notification.Id,
            TenantId = tenantId.Value,
            Channel = channel.ToString(),
            RecipientAddress = recipient.Address,
            OccurredAt = now
        });

        return notification;
    }

    public void MarkAsProcessing()
    {
        if (Status != NotificationStatus.Pending && Status != NotificationStatus.Retrying)
            throw new NotificationDomainException(
                $"Cannot process notification with status {Status}.");

        Status = NotificationStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != NotificationStatus.Processing)
            throw new NotificationDomainException(
                $"Cannot mark as delivered from status {Status}.");

        Status = NotificationStatus.Delivered;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new NotificationDomainException("Failure reason cannot be empty.");

        Status = NotificationStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRetrying(string reason)
    {
        if (Status != NotificationStatus.Processing)
            throw new NotificationDomainException(
                $"Cannot retry notification with status {Status}.");

        RetryCount++;
        Status = NotificationStatus.Retrying;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool CanRetry(int maxRetries = 3) => RetryCount < maxRetries;
}
