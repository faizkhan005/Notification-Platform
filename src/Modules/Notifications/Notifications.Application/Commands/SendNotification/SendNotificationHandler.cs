using BuildingBlocks.Application;
using Notifications.Domain;
using Preferences.Domain;
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
    private readonly IPreferenceRepository _preferenceRepository;

    public SendNotificationHandler(
        INotificationRepository repository,
        INotificationsUnitOfWork unitOfWork,
        ITenantRepository tenantRepository,
        ITemplateRepository templateRepository,
        IPreferenceRepository preferenceRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
        _templateRepository = templateRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<SendNotificationResponse> Handle(
        SendNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(command.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken) 
            ?? throw new Domain.Exceptions.NotificationDomainException($"Tenant '{command.TenantId}' not found.");

        if (!tenant.CanSendNotifications())
            throw new Domain.Exceptions.NotificationDomainException($"Tenant '{command.TenantId}' is not active.");

        var channel = Enum.Parse<NotificationChannel>(command.Channel, ignoreCase: true);

        await EnsureRecipientHasNotOptedOutAsync(tenantId, command.RecipientAddress, channel, cancellationToken);

        var recipient = Recipient.Create(command.RecipientAddress, command.RecipientName);
        var (subject, body) = await ResolveContentAsync(command, tenantId, cancellationToken);
        var content = NotificationContent.Create(subject, body);
        var notification = Notification.Create(tenantId, channel, recipient, content);

        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendNotificationResponse(notification.Id.Value, notification.Status.ToString(), notification.CreatedAt);
    }

    private async Task EnsureRecipientHasNotOptedOutAsync(
        TenantId tenantId, string recipientAddress, NotificationChannel channel, CancellationToken cancellationToken)
    {
        // Map Notifications' channel enum to Preferences' own channel enum —
        // both modules define their own to avoid a forbidden cross-module
        // dependency, so we translate at the boundary where they meet.
        var preferenceChannel = (Preferences.Domain.Channel)(int)channel;

        var preference = await _preferenceRepository.GetAsync(
            tenantId, recipientAddress, preferenceChannel, cancellationToken);

        if (preference?.IsOptedOut == true)
            throw new Domain.Exceptions.NotificationDomainException(
                $"Recipient '{recipientAddress}' has opted out of {channel} notifications.");
    }

    private async Task<(string Subject, string Body)> ResolveContentAsync(
        SendNotificationCommand command, TenantId tenantId, CancellationToken cancellationToken)
    {
        if (!command.TemplateId.HasValue)
            return (command.Subject!, command.Body!);

        var templateId = TemplateId.From(command.TemplateId.Value);
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new Domain.Exceptions.NotificationDomainException($"Template '{command.TemplateId}' not found.");

        if (template.TenantId != tenantId)
            throw new Domain.Exceptions.NotificationDomainException(
                $"Template '{command.TemplateId}' does not belong to tenant '{tenantId.Value}'.");

        var variables = command.TemplateVariables ?? new Dictionary<string, string>();
        return template.Render(variables);
    }
}
