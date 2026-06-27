using System.Text.RegularExpressions;
using Tenants.Domain.Exceptions;

namespace Tenants.Domain;

public sealed record TenantSlug
{
    private static readonly Regex ValidSlugPattern =
        new(@"^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$", RegexOptions.Compiled);

    public string Value { get; }

    private TenantSlug(string value) => Value = value;

    public static TenantSlug Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TenantDomainException("Tenant slug cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!ValidSlugPattern.IsMatch(normalized))
            throw new TenantDomainException(
                "Slug must be 3-50 characters, lowercase alphanumeric and hyphens only, cannot start or end with a hyphen.");

        return new TenantSlug(normalized);
    }

    public static implicit operator string(TenantSlug slug) => slug.Value;
    public override string ToString() => Value;
}

