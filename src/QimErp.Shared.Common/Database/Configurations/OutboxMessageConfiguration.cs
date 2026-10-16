namespace QimErp.Shared.Common.Database.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType);

        builder.Property(e => e.Payload);

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        builder.Property(e => e.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => new { e.IsProcessed, e.OccurredAt })
            .HasDatabaseName("IX_OutboxMessages_IsProcessed_OccurredAt");
    }
}
