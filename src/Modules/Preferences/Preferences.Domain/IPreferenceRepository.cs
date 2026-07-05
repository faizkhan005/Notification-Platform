using Tenants.Domain;

namespace Preferences.Domain;

public interface IPreferenceRepository
{
    Task<Preference?> GetAsync(
        TenantId tenantId,
        string recipientAddress,
        Channel channel,
        CancellationToken cancellationToken = default);

    Task AddAsync(Preference preference, CancellationToken cancellationToken = default);
}