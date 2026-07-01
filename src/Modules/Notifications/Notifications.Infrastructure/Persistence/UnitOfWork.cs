using BuildingBlocks.Infrastructure.Outbox;
using MediatR;
using Notifications.Application;
using Notifications.Domain;
using System.Text.Json;

namespace Notifications.Infrastructure.Persistence;

public sealed class UnitOfWork : INotificationsUnitOfWork
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

        // Write each domain event as an OutboxMessage row BEFORE SaveChanges.
        // This is the key moment: the outbox row is added to the SAME
        // change tracker as the Notification entity. When SaveChangesAsync
        // runs below, BOTH the notification insert and the outbox insert
        // happen in the SAME database transaction. Either both succeed
        // or both roll back — there is no window where one exists without the other.
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessage.Create(
                type: domainEvent.GetType().AssemblyQualifiedName!,
                content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

            _context.OutboxMessages.Add(outboxMessage);
        }


        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var entry in _context.ChangeTracker.Entries<Notification>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
