using BuildingBlocks.Domain;
using Tenants.Domain.Events;
using Tenants.Domain.Exceptions;

namespace Tenants.Domain;

public sealed class Tenant
{
    public TenantId Id { get; private set; }
    public TenantName Name { get; private set; }
    public TenantSlug Slug { get; private set; }
    public Plan Plan { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Tenant()
    {
        Name = null!;
        Slug = null!;
    }

    public static Tenant Create(TenantName name, TenantSlug slug, Plan plan)
    {
        var now = DateTimeOffset.UtcNow;

        var tenant = new Tenant
        {
            Id = TenantId.New(),
            Name = name,
            Slug = slug,
            Plan = plan,
            Status = TenantStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        tenant._domainEvents.Add(new TenantCreated
        {
            TenantId = tenant.Id,
            Name = name.Value,
            Slug = slug.Value,
            Plan = plan,
            OccurredAt = now
        });

        return tenant;
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Deleted)
            throw new TenantDomainException($"Cannot suspend deleted tenant '{Slug}'.");

        if (Status == TenantStatus.Suspended) return;

        Status = TenantStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reinstate()
    {
        if (Status != TenantStatus.Suspended)
            throw new TenantDomainException($"Cannot reinstate tenant with status {Status}.");

        Status = TenantStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        if (Status == TenantStatus.Deleted)
            throw new TenantDomainException($"Tenant '{Slug}' is already deleted.");

        Status = TenantStatus.Deleted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpgradePlan(Plan newPlan)
    {
        if (Status == TenantStatus.Deleted)
            throw new TenantDomainException($"Cannot change plan for deleted tenant.");

        if (newPlan <= Plan)
            throw new TenantDomainException($"Cannot downgrade from {Plan} to {newPlan} via upgrade.");

        Plan = newPlan;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool CanSendNotifications() => Status == TenantStatus.Active;
}
