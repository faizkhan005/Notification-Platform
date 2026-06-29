namespace Tenants.Application;

public interface ITenantsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
