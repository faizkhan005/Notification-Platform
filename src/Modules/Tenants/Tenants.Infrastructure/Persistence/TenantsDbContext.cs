using Microsoft.EntityFrameworkCore;
using Tenants.Domain;

namespace Tenants.Infrastructure.Persistence;

public sealed class TenantsDbContext : DbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public TenantsDbContext(DbContextOptions<TenantsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantsDbContext).Assembly);
    }
}
