using FluentValidation;
using Notifications.Domain;

namespace Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .Must(c => Enum.TryParse<NotificationChannel>(c, ignoreCase: true, out _))
            .WithMessage($"Channel must be one of: {string.Join(", ", Enum.GetNames<NotificationChannel>())}.");

        RuleFor(x => x.RecipientAddress)
            .NotEmpty().WithMessage("Recipient address is required.")
            .MaximumLength(320).WithMessage("Recipient address cannot exceed 320 characters.");

        // Exactly one of (Subject+Body) or (TemplateId) must be provided — not both, not neither.
        RuleFor(x => x)
            .Must(x => (x.TemplateId.HasValue) ^ (!string.IsNullOrWhiteSpace(x.Subject) && !string.IsNullOrWhiteSpace(x.Body)))
            .WithMessage("Provide either TemplateId (with TemplateVariables) OR both Subject and Body, not both modes at once.");
    }
}
