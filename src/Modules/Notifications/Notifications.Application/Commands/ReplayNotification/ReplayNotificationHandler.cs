using BuildingBlocks.Application;
using Notifications.Domain;
using Notifications.Domain.Exceptions;

namespace Notifications.Application.Commands.ReplayNotification;

public sealed class ReplayNotificationHandler
    : ICommandHandler<ReplayNotificationCommand, ReplayNotificationResponse>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationsUnitOfWork _unitOfWork;

    public ReplayNotificationHandler(
        INotificationRepository repository,
        INotificationsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReplayNotificationResponse> Handle(
        ReplayNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var id = NotificationId.From(command.NotificationId);
        var notification = await _repository.GetByIdAsync(id, cancellationToken);

        if (notification is null)
            throw new NotificationDomainException($"Notification '{command.NotificationId}' not found.");

        notification.ResetForReplay();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReplayNotificationResponse(notification.Id.Value, notification.Status.ToString());
    }
}
