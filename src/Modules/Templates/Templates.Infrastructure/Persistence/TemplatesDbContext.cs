using Microsoft.EntityFrameworkCore;
using Templates.Domain;

namespace Templates.Infrastructure.Persistence;

public sealed class TemplatesDbContext : DbContext
{
    public DbSet<Template> Templates => Set<Template>();

    public TemplatesDbContext(DbContextOptions<TemplatesDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplatesDbContext).Assembly);
    }
}
