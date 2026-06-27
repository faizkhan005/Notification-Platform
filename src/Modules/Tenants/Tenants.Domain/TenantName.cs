using Tenants.Domain.Exceptions;

namespace Tenants.Domain;

public sealed record TenantName
{
    public string Value { get; }

    private TenantName(string value) => Value = value;

    public static TenantName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TenantDomainException("Tenant name cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > 100)
            throw new TenantDomainException("Tenant name cannot exceed 100 characters.");

        return new TenantName(trimmed);
    }

    public static implicit operator string(TenantName name) => name.Value;
    public override string ToString() => Value;
}
