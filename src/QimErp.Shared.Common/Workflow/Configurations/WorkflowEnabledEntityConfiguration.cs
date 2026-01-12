namespace QimErp.Shared.Common.Workflow.Configurations;

public abstract class WorkflowEnabledEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : WorkflowEnabledEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.DataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.PreviousDataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.WorkflowStatus).HasConversion(new EnumToStringConverter<WorkflowStatus>());
        builder.Property(e => e.CustomFields).HasColumnType("jsonb").IsRequired(false);


        builder.HasIndex(e => e.DataStatus); // For filtering by status
        builder.HasIndex(e => e.Created); // For sorting by creation date
        builder.HasIndex(e => e.LastModified); 
        builder.HasIndex(e => e.TenantId); 
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

        ConfigureWorkflowDefinition(builder);

    }

    public void ConfigureWorkflowDefinition<TEntity>(EntityTypeBuilder<TEntity> builder)
       where TEntity : WorkflowEnabledEntity
    {
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
                esc.Property(e => e.EscalationMessage).HasMaxLength(1000);
                esc.Property(e => e.RepeatEscalation);
                esc.Property(e => e.RepeatIntervalDays);
                esc.Property(e => e.EscalateTo);
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
    }
}

