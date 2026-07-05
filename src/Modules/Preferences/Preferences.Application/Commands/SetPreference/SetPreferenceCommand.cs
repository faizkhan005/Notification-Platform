using BuildingBlocks.Application;

namespace Preferences.Application.Commands.SetPreference;

public sealed record SetPreferenceCommand(
    Guid TenantId,
    string RecipientAddress,
    string Channel,
    bool OptedOut
) : ICommand<SetPreferenceResponse>;
