using Templates.Application;

namespace Templates.Infrastructure.Persistence;

public sealed class UnitOfWork : ITemplatesUnitOfWork
{
    private readonly TemplatesDbContext _context;

    public UnitOfWork(TemplatesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
