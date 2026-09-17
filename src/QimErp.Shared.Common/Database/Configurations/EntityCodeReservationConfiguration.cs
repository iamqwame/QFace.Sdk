namespace QimErp.Shared.Common.Database.Configurations;

public class EntityCodeReservationConfiguration : AuditableEntityConfiguration<EntityCodeReservation>
{
    public override void Configure(EntityTypeBuilder<EntityCodeReservation> builder)
    {
        base.Configure(builder);

        builder.ToTable("EntityCodeReservations");

        builder.Property(e => e.EntityType).IsRequired();
        builder.Property(e => e.Code).IsRequired();
        builder.Property(e => e.ReservationToken).IsRequired();
        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.Metadata);

        // Soft-deleted rows release the code; only Active rows participate in uniqueness.
        builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.EntityType, e.Code })
            .IsUnique()
            .HasFilter("\"DataStatus\" = 'Active'")
            .HasDatabaseName("IX_EntityCodeReservations_TenantId_CompanyId_EntityType_Code");
    }
}
