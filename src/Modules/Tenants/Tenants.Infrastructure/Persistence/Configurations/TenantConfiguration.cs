using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenants.Domain;

namespace Tenants.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasConversion(name => name.Value, value => TenantName.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .HasConversion(slug => slug.Value, value => TenantSlug.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Plan)
            .HasColumnName("plan")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tenants_status");

        builder.HasQueryFilter(t => t.Status != TenantStatus.Deleted);

        builder.Ignore(t => t.DomainEvents);
    }
}
