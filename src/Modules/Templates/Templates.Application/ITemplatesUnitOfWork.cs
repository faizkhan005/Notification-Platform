namespace Templates.Application;

public interface ITemplatesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
