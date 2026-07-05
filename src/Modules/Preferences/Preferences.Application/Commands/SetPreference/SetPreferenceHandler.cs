using BuildingBlocks.Application;
using Preferences.Domain;
using Tenants.Domain;

namespace Preferences.Application.Commands.SetPreference;

public sealed class SetPreferenceHandler : ICommandHandler<SetPreferenceCommand, SetPreferenceResponse>
{
    private readonly IPreferenceRepository _repository;
    private readonly IPreferencesUnitOfWork _unitOfWork;

    public SetPreferenceHandler(IPreferenceRepository repository, IPreferencesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetPreferenceResponse> Handle(
        SetPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(command.TenantId);
        var channel = Enum.Parse<Channel>(command.Channel, ignoreCase: true);

        var existing = await _repository.GetAsync(
            tenantId, command.RecipientAddress, channel, cancellationToken);

        var isOptedOut = await ApplyPreferenceAsync(
            existing, tenantId, command.RecipientAddress, channel, command.OptedOut, cancellationToken);

        return new SetPreferenceResponse(
            command.TenantId, command.RecipientAddress, command.Channel, isOptedOut);
    }

    private async Task<bool> ApplyPreferenceAsync(
        Preference? existing,
        TenantId tenantId,
        string recipientAddress,
        Channel channel,
        bool wantsOptedOut,
        CancellationToken cancellationToken)
    {
        if (existing is null && !wantsOptedOut)
            return false; // no row, wants opted-in: already the default, nothing to persist

        if (existing is null && wantsOptedOut)
        {
            var preference = Preference.OptOut(tenantId, recipientAddress, channel);
            await _repository.AddAsync(preference, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (existing!.IsOptedOut && !wantsOptedOut)
        {
            existing.OptIn();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return existing.IsOptedOut;
    }
}
