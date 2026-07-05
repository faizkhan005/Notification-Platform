namespace Preferences.Application.Commands.SetPreference;

public sealed record SetPreferenceResponse(
    Guid TenantId,
    string RecipientAddress,
    string Channel,
    bool IsOptedOut
);
