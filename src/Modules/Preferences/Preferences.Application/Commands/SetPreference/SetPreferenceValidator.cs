using FluentValidation;
using Preferences.Domain;

namespace Preferences.Application.Commands.SetPreference;

public sealed class SetPreferenceValidator : AbstractValidator<SetPreferenceCommand>
{
    public SetPreferenceValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.RecipientAddress)
            .NotEmpty().WithMessage("Recipient address is required.")
            .MaximumLength(320);

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .Must(c => Enum.TryParse<Channel>(c, ignoreCase: true, out _))
            .WithMessage($"Channel must be one of: {string.Join(", ", Enum.GetNames<Channel>())}.");
    }
}
