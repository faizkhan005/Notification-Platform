using Microsoft.EntityFrameworkCore;
using Preferences.Domain;
using Tenants.Domain;

namespace Preferences.Infrastructure.Persistence;

public sealed class PreferenceRepository : IPreferenceRepository
{
    private readonly PreferencesDbContext _context;

    public PreferenceRepository(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Preference?> GetAsync(
        TenantId tenantId,
        string recipientAddress,
        Channel channel,
        CancellationToken cancellationToken = default)
    {
        var normalized = recipientAddress.Trim().ToLowerInvariant();

        return await _context.Preferences.FirstOrDefaultAsync(
            p => p.TenantId == tenantId
                 && p.RecipientAddress == normalized
                 && p.Channel == channel,
            cancellationToken);
    }

    public async Task AddAsync(Preference preference, CancellationToken cancellationToken = default)
        => await _context.Preferences.AddAsync(preference, cancellationToken);
}
