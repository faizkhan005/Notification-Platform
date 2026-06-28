using Microsoft.EntityFrameworkCore;
using Tenants.Domain;

namespace Tenants.Infrastructure.Persistence;

public sealed class TenantRepository : ITenantRepository
{
    private readonly TenantsDbContext _context;

    public TenantRepository(TenantsDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default)
        => await _context.Tenants.FindAsync([id], cancellationToken);

    public async Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
        => await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

    public async Task<bool> ExistsBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
        => await _context.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => await _context.Tenants.AddAsync(tenant, cancellationToken);
}