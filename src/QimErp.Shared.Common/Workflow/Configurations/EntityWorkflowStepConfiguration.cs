namespace QimErp.Shared.Common.Workflow.Configurations;

public class EntityWorkflowStepConfiguration : IEntityTypeConfiguration<EntityWorkflowStep>
{
    public void Configure(EntityTypeBuilder<EntityWorkflowStep> builder)
    {
        builder.ToTable("EntityWorkflowSteps");

        // Configure string properties
        builder.Property(e => e.WorkflowCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);

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
                step.Property(s => s.StepCode).HasMaxLength(100);
                step.OwnsMany(s => s.RequiredApprovers, approver =>
                {
                    approver.Property(a => a.Type).HasMaxLength(50);
                    approver.Property(a => a.ValueId).HasMaxLength(100);
                    approver.Property(a => a.Value).HasMaxLength(200);
                });
                step.OwnsMany(s => s.Conditions, condition =>
                {
                    condition.Property(c => c.Field).HasMaxLength(100);
                    condition.Property(c => c.Value).HasMaxLength(500);
                    condition.Property(c => c.Operator);
                    condition.Property(c => c.Logic);
                });

                step.OwnsOne(s => s.OnApproval, action =>
                {
                    action.Property(a => a.NextStepCode).HasMaxLength(200);
                    action.Property(a => a.CompleteWorkflow);
                    action.Property(a => a.SendNotificationTo);
                    action.Property(a => a.SendEmailTo);
                });

                step.OwnsOne(s => s.OnRejection, action =>
                {
                    action.Property(a => a.NextStepCode).HasMaxLength(200);
                    action.Property(a => a.CompleteWorkflow);
                    action.Property(a => a.SendNotificationTo);
                    action.Property(a => a.SendEmailTo);
                });
            });

            // Configure Notifications
            wd.OwnsOne(w => w.Notifications, notif =>
            {
                notif.Property(n => n.SendEmailNotifications);
                notif.Property(n => n.SendSmsNotifications);
                notif.Property(n => n.OnStart);
                notif.Property(n => n.OnApproval);
                notif.Property(n => n.OnRejection);
                notif.Property(n => n.OnCompletion);
                notif.Property(n => n.OnTimeout);
            });

            // Configure Escalation
            wd.OwnsOne(w => w.Escalation, esc =>
            {
                esc.Property(e => e.Enabled);
                esc.Property(e => e.EscalateAfterDays);
                esc.Property(e => e.EscalateTo);
                esc.Property(e => e.EscalationMessage).HasMaxLength(500);
                esc.Property(e => e.RepeatEscalation);
                esc.Property(e => e.RepeatIntervalDays);
            });

            // Configure AutoApproval
            wd.OwnsOne(w => w.AutoApproval, auto =>
            {
                auto.Property(a => a.Enabled);
                auto.Property(a => a.AutoApprovalReason).HasMaxLength(500);
                auto.Property(a => a.NotifyOnAutoApproval);
                auto.OwnsMany(a => a.Conditions, condition =>
                {
                    condition.Property(c => c.Field).HasMaxLength(100);
                    condition.Property(c => c.Value).HasMaxLength(500);
                    condition.Property(c => c.Operator);
                    condition.Property(c => c.Logic);
                });
            });

            // Configure Timeout
            wd.OwnsOne(w => w.Timeout, timeout =>
            {
                timeout.Property(t => t.DefaultTimeoutDays);
                timeout.Property(t => t.TimeoutAction);
                timeout.Property(t => t.TimeoutReason).HasMaxLength(500);
                timeout.Property(t => t.NotifyOnTimeout);
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

        // Configure DataStatus
        builder.Property(e => e.DataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.PreviousDataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.HasIndex(e => e.DataStatus);
        builder.HasIndex(e => new { e.DataStatus, e.IsActive });
    }
}

