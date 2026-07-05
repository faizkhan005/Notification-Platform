using Preferences.Application;

namespace Preferences.Infrastructure.Persistence;

public sealed class UnitOfWork : IPreferencesUnitOfWork
{
    private readonly PreferencesDbContext _context;

    public UnitOfWork(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
