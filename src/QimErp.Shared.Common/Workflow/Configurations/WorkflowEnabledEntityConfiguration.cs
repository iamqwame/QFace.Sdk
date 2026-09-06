namespace QimErp.Shared.Common.Workflow.Configurations;

public abstract class WorkflowEnabledEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : WorkflowEnabledEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.DataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.PreviousDataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.WorkflowStatus).HasConversion(new EnumToStringConverter<WorkflowStatus>());
        builder.Property(e => e.CustomFields).HasColumnType("jsonb");


        builder.HasIndex(e => e.DataStatus); // For filtering by status
        builder.HasIndex(e => e.Created); // For sorting by creation date
        builder.HasIndex(e => e.LastModified);
        builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.DataStatus });
        builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.WorkflowStatus });
        builder.HasIndex(e => new { e.DataStatus, e.Created });
        builder.HasIndex(e => new { e.DataStatus, e.LastModified }); // Composite for recent changes


        // Workflow-specific indexes
        builder.HasIndex(e => e.WorkflowStatus); // For filtering by workflow status
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus }); // Active records by workflow status
        builder.HasIndex(e => new { e.WorkflowStatus, e.Created }); // Workflow status by creation date
        builder.HasIndex(e => new { e.WorkflowStatus, e.LastModified }); // Workflow status by last modified
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus, e.Created }); // Active records by workflow and creation
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus, e.LastModified }); // Active records by workflow and modification


        builder.Ignore(e => e.IsActive);
        builder.Ignore(e=>e.IsPendingApproval);
        builder.Ignore(e=>e.IsRejected);
        builder.Ignore(e=>e.IsWorkflowComplete);
        builder.Ignore(e=>e.EntityType);
        builder.Ignore(e => e.IsWorkflowEnabled);

    }
}

