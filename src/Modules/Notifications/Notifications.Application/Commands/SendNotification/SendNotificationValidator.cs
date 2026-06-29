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

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");
    }
}
