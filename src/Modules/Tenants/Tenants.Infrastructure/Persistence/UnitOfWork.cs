using BuildingBlocks.Application;
using MediatR;
using Tenants.Domain;

namespace Tenants.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TenantsDbContext _context;
    private readonly IPublisher _publisher;

    public UnitOfWork(TenantsDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = _context.ChangeTracker
            .Entries<Tenant>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var entry in _context.ChangeTracker.Entries<Tenant>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
