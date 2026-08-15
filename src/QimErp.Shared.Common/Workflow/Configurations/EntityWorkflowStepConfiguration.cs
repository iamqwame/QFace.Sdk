using QimErp.Shared.Common.Database.Configurations;

namespace QimErp.Shared.Common.Workflow.Configurations;

public class EntityWorkflowStepConfiguration : AuditableEntityConfiguration<EntityWorkflowStep>
{
    public override void Configure(EntityTypeBuilder<EntityWorkflowStep> builder)
    {
        base.Configure(builder);
        builder.ToTable("EntityWorkflowSteps");

        // Configure string properties
        builder.Property(e => e.WorkflowCode)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.Category)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Configure WorkflowDefinition as JSON column
        builder.OwnsOne(e => e.WorkflowDefinition, wd =>
        {
            wd.ToJson("WorkflowDefinition");

            // Configure the Steps collection
            wd.OwnsMany(w => w.Steps, step =>
            {
                step.OwnsMany(s => s.RequiredApprovers, approver =>
                {
                });
                step.OwnsMany(s => s.Conditions, condition =>
                {
                });

                step.OwnsOne(s => s.OnApproval, action =>
                {
                });

                step.OwnsOne(s => s.OnRejection, action =>
                {
                });
            });

            // Notifications — must be OwnsOne so EF Core treats it as a JSON-owned type,
            // not a separate entity requiring a primary key.
            wd.OwnsOne(w => w.Notifications, notif =>
            {
                notif.Property(n => n.SendEmailNotifications);
                notif.Property(n => n.SendSmsNotifications);
            });

            // Configure Escalation
            wd.OwnsOne(w => w.Escalation, esc =>
            {
            });

            // Timeout — must be OwnsOne for the same reason as Notifications.
            wd.OwnsOne(w => w.Timeout, timeout =>
            {
            });

            // Configure AutoApproval
            wd.OwnsOne(w => w.AutoApproval, auto =>
            {
                auto.OwnsMany(a => a.Conditions, condition =>
                {
                });
            });
        });

        // Configure indexes
        builder.HasIndex(e => e.WorkflowCode);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.WorkflowCode, e.EntityType });
        builder.HasIndex(e => new { e.EntityType, e.IsActive });
        builder.HasIndex(e => new { e.WorkflowCode, e.IsActive });
        builder.HasIndex(e => new { e.WorkflowCode, e.EntityType, e.IsActive });

        builder.HasIndex(e => new { e.DataStatus, e.IsActive });
    }
}

