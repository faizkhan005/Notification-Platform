using Templates.Domain.Exceptions;

namespace Templates.Domain;

public sealed record TemplateName
{
    public string Value { get; }

    private TemplateName(string value) => Value = value;

    public static TemplateName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TemplateDomainException("Template name cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > 150)
            throw new TemplateDomainException("Template name cannot exceed 150 characters.");

        return new TemplateName(trimmed);
    }

    public override string ToString() => Value;
}

