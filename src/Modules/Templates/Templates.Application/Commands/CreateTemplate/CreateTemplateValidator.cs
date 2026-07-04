using FluentValidation;

namespace Templates.Application.Commands.CreateTemplate;

public sealed class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(150).WithMessage("Template name cannot exceed 150 characters.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");
    }
}
