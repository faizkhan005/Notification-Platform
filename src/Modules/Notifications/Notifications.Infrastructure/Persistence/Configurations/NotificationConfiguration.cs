using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain;
using Tenants.Domain;

namespace Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => NotificationId.From(value))
            .IsRequired();

        builder.Property(n => n.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(n => n.Channel)
            .HasColumnName("channel")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(n => n.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(n => n.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(1000);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Recipient is a value object — stored as owned entity (flattened into same table)
        builder.OwnsOne(n => n.Recipient, recipient =>
        {
            recipient.Property(r => r.Address)
                .HasColumnName("recipient_address")
                .HasMaxLength(320)
                .IsRequired();

            recipient.Property(r => r.Name)
                .HasColumnName("recipient_name")
                .HasMaxLength(200);
        });

        // NotificationContent stored as owned entity
        builder.OwnsOne(n => n.Content, content =>
        {
            content.Property(c => c.Subject)
                .HasColumnName("subject")
                .HasMaxLength(500)
                .IsRequired();

            content.Property(c => c.Body)
                .HasColumnName("body")
                .IsRequired();
        });

        builder.HasIndex(n => n.TenantId)
            .HasDatabaseName("ix_notifications_tenant_id");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_notifications_status");

        builder.HasIndex(n => new { n.TenantId, n.Status })
            .HasDatabaseName("ix_notifications_tenant_id_status");

        builder.Ignore(n => n.DomainEvents);
    }
}
