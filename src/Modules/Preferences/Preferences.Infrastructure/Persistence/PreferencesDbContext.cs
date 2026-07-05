using Microsoft.EntityFrameworkCore;
using Preferences.Domain;

namespace Preferences.Infrastructure.Persistence;

public sealed class PreferencesDbContext : DbContext
{
    public DbSet<Preference> Preferences => Set<Preference>();

    public PreferencesDbContext(DbContextOptions<PreferencesDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PreferencesDbContext).Assembly);
    }
}
