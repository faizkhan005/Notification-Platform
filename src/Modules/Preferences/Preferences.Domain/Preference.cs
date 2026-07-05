using Preferences.Domain.Exceptions;
using Tenants.Domain;

namespace Preferences.Domain;

public sealed class Preference
{
    public PreferenceId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public string RecipientAddress { get; private set; } = null!;
    public Channel Channel { get; private set; }
    public bool IsOptedOut { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Preference() { }

    public static Preference OptOut(TenantId tenantId, string recipientAddress, Channel channel)
    {
        if (string.IsNullOrWhiteSpace(recipientAddress))
            throw new PreferenceDomainException("Recipient address cannot be empty.");

        var now = DateTimeOffset.UtcNow;

        return new Preference
        {
            Id = PreferenceId.New(),
            TenantId = tenantId,
            RecipientAddress = recipientAddress.Trim().ToLowerInvariant(),
            Channel = channel,
            IsOptedOut = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Reverses an opt-out. Rather than deleting the row, we flip the flag —
    /// this preserves an audit trail of "this recipient opted out, then back in,"
    /// which matters for compliance disputes.
    /// </summary>
    public void OptIn()
    {
        IsOptedOut = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}