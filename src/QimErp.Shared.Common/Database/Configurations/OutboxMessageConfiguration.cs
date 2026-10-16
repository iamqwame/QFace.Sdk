namespace QimErp.Shared.Common.Database.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Transport).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.LockedBy).HasMaxLength(128);
        builder.Property(x => x.LastError).HasMaxLength(2000);

#pragma warning disable CS0618 // IsProcessed is obsolete; ignore so EF does not map it.
        builder.Ignore(x => x.IsProcessed);
#pragma warning restore CS0618

        builder.HasIndex(x => x.NextAttemptAt)
            .HasDatabaseName("ix_outbox_ready")
            .HasFilter("\"ProcessedAt\" IS NULL AND \"DeadAt\" IS NULL");

        builder.HasIndex(x => new { x.EntityId, x.OccurredAt })
            .HasDatabaseName("ix_outbox_entity_order");
    }
}
