using BuildingBlocks.Application;
using Notifications.Domain;
using Templates.Domain;
using Tenants.Domain;

namespace Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationHandler
    : ICommandHandler<SendNotificationCommand, SendNotificationResponse>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationsUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITemplateRepository _templateRepository;

    public SendNotificationHandler(
        INotificationRepository repository,
        INotificationsUnitOfWork unitOfWork,
        ITenantRepository tenantRepository,
        ITemplateRepository templateRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
        _templateRepository = templateRepository;
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

        // Resolve subject/body — either from a template (rendered with variables)
        // or directly from the raw fields on the command.
        string subject;
        string body;
        if (command.TemplateId.HasValue)
        {
            var templateId = TemplateId.From(command.TemplateId.Value);
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);

            if (template is null)
                throw new Domain.Exceptions.NotificationDomainException(
                    $"Template '{command.TemplateId}' not found.");

            // Cross-tenant safety: a tenant must never be able to send using
            // another tenant's template. This is a critical multi-tenancy
            // invariant enforced right here, at the point of use.
            if (template.TenantId != tenantId)
                throw new Domain.Exceptions.NotificationDomainException(
                    $"Template '{command.TemplateId}' does not belong to tenant '{command.TenantId}'.");

            var variables = command.TemplateVariables ?? new Dictionary<string, string>();
            (subject, body) = template.Render(variables);
        }
        else
        {
            subject = command.Subject!;
            body = command.Body!;
        }

        var content = NotificationContent.Create(subject, body);

        var notification = Notification.Create(tenantId, channel, recipient, content);

        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendNotificationResponse(
            notification.Id.Value,
            notification.Status.ToString(),
            notification.CreatedAt);
    }
}
