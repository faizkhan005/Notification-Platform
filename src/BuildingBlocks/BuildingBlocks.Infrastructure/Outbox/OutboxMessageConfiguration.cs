using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Outbox;


public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz");

        builder.Property(m => m.Error)
            .HasColumnName("error")
            .HasMaxLength(2000);

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        // This is the index that matters most — the processor's query is
        // "find unprocessed messages", which is WHERE processed_at IS NULL.
        // Without this index, that becomes a full table scan as the table grows.
        builder.HasIndex(m => m.ProcessedAt)
            .HasDatabaseName("ix_outbox_messages_processed_at");
    }
}

