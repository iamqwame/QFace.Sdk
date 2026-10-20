using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QimErp.Shared.Common.Database.Configurations;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Workflow.Configurations;

public abstract class WorkflowEnabledEntityConfiguration<TEntity> : AuditableEntityConfiguration<TEntity>
    where TEntity : WorkflowEnabledEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // DATA-20: inherit Version concurrency token from AuditableEntityConfiguration.
        base.Configure(builder);

        builder.Property(e => e.WorkflowStatus).HasConversion(new EnumToStringConverter<WorkflowStatus>());

        builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.WorkflowStatus });
        builder.HasIndex(e => e.WorkflowStatus);
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus });
        builder.HasIndex(e => new { e.WorkflowStatus, e.Created });
        builder.HasIndex(e => new { e.WorkflowStatus, e.LastModified });
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus, e.Created });
        builder.HasIndex(e => new { e.DataStatus, e.WorkflowStatus, e.LastModified });

        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.IsPendingApproval);
        builder.Ignore(e => e.IsRejected);
        builder.Ignore(e => e.IsWorkflowComplete);
        builder.Ignore(e => e.EntityType);
        builder.Ignore(e => e.IsWorkflowEnabled);
    }
}
