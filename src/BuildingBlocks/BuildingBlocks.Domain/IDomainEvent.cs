using MediatR;

namespace BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}
