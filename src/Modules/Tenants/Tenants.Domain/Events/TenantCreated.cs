using BuildingBlocks.Domain;

namespace Tenants.Domain.Events;

public sealed record TenantCreated : IDomainEvent
{
    public required TenantId TenantId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required Plan Plan { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
