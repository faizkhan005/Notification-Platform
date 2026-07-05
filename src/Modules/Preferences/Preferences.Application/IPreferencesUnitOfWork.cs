namespace Preferences.Application;

public interface IPreferencesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
