namespace Tenants.Domain;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
