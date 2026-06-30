using BuildingBlocks.Application;
using Notifications.Domain;

namespace Notifications.Application.Queries.GetNotificationById;

public sealed class GetNotificationByIdHandler
    : IQueryHandler<GetNotificationByIdQuery, NotificationResponse?>
{
    private readonly INotificationRepository _repository;

    public GetNotificationByIdHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationResponse?> Handle(
        GetNotificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var id = NotificationId.From(query.Id);
        var notification = await _repository.GetByIdAsync(id, cancellationToken);

        if (notification is null) return null;

        return new NotificationResponse(
            notification.Id.Value,
            notification.TenantId.Value,
            notification.Channel.ToString(),
            notification.Recipient.Address,
            notification.Recipient.Name,
            notification.Content.Subject,
            notification.Status.ToString(),
            notification.FailureReason,
            notification.RetryCount,
            notification.CreatedAt,
            notification.UpdatedAt);
    }
}
