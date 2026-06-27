using FluentValidation;

namespace Tenants.Application.Commands.CreateTenant;

public sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Tenant slug is required.")
            .Length(3, 50).WithMessage("Slug must be between 3 and 50 characters.")
            .Matches(@"^[a-z0-9][a-z0-9-]*[a-z0-9]$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens, cannot start or end with a hyphen.");

        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .Must(p => Enum.TryParse<Domain.Plan>(p, ignoreCase: true, out _))
            .WithMessage($"Plan must be one of: {string.Join(", ", Enum.GetNames<Domain.Plan>())}.");
    }
}
