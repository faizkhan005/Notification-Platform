namespace Preferences.Api.Contracts;

public sealed record SetPreferenceRequest(
    Guid TenantId,
    string RecipientAddress,
    string Channel,
    bool OptedOut
);
