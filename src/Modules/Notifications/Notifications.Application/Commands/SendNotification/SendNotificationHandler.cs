using BuildingBlocks.Application;
using Notifications.Domain;
using Tenants.Domain;

namespace Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationHandler
    : ICommandHandler<SendNotificationCommand, SendNotificationResponse>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;

    public SendNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        ITenantRepository tenantRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
    }

    public async Task<SendNotificationResponse> Handle(
        SendNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(command.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new Domain.Exceptions.NotificationDomainException(
                $"Tenant '{command.TenantId}' not found.");

        if (!tenant.CanSendNotifications())
            throw new Domain.Exceptions.NotificationDomainException(
                $"Tenant '{command.TenantId}' is not active.");

        var channel = Enum.Parse<NotificationChannel>(command.Channel, ignoreCase: true);
        var recipient = Recipient.Create(command.RecipientAddress, command.RecipientName);
        var content = NotificationContent.Create(command.Subject, command.Body);

        var notification = Notification.Create(tenantId, channel, recipient, content);

        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendNotificationResponse(
            notification.Id.Value,
            notification.Status.ToString(),
            notification.CreatedAt);
    }
}
