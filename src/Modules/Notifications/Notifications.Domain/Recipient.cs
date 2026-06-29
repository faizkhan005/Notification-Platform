using Notifications.Domain.Exceptions;

namespace Notifications.Domain;

public sealed record Recipient
{
    public string Address { get; }
    public string? Name { get; }

    private Recipient(string address, string? name)
    {
        Address = address;
        Name = name;
    }

    public static Recipient Create(string? address, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new NotificationDomainException("Recipient address cannot be empty.");

        if (address.Length > 320)
            throw new NotificationDomainException("Recipient address cannot exceed 320 characters.");

        return new Recipient(address.Trim().ToLowerInvariant(), name?.Trim());
    }

    public override string ToString() => Name is null ? Address : $"{Name} <{Address}>";
}

