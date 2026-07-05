using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Preferences.Domain;
using Tenants.Domain;

namespace Preferences.Infrastructure.Persistence.Configurations;

public sealed class PreferenceConfiguration : IEntityTypeConfiguration<Preference>
{
    public void Configure(EntityTypeBuilder<Preference> builder)
    {
        builder.ToTable("preferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PreferenceId.From(value))
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(p => p.RecipientAddress)
            .HasColumnName("recipient_address")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(p => p.Channel)
            .HasColumnName("channel")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.IsOptedOut)
            .HasColumnName("is_opted_out")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Unique constraint: one preference row per (tenant, recipient, channel).
        // This is the natural key for this aggregate — the repository's GetAsync
        // relies on this being unique to make sense as a single-result lookup.
        builder.HasIndex(p => new { p.TenantId, p.RecipientAddress, p.Channel })
            .IsUnique()
            .HasDatabaseName("ix_preferences_tenant_recipient_channel");
    }
}
