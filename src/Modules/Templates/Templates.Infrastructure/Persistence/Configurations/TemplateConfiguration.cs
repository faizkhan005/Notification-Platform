using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Templates.Domain;
using Tenants.Domain;

namespace Templates.Infrastructure.Persistence.Configurations;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TemplateId.From(value))
            .IsRequired();

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.OwnsOne(t => t.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();
        });

        builder.OwnsOne(t => t.Content, content =>
        {
            content.Property(c => c.Subject)
                .HasColumnName("subject")
                .HasMaxLength(500)
                .IsRequired();

            content.Property(c => c.Body)
                .HasColumnName("body")
                .IsRequired();
        });

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_templates_tenant_id");
    }
}