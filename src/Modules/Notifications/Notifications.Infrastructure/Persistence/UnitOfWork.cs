using BuildingBlocks.Application;
using MediatR;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NotificationsDbContext _context;
    private readonly IPublisher _publisher;

    public UnitOfWork(NotificationsDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = _context.ChangeTracker
            .Entries<Notification>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var entry in _context.ChangeTracker.Entries<Notification>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
