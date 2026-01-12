namespace QimErp.Shared.Common.Services.Workflow;

public interface IWorkflowApprovalProcessor
{
    Task ProcessApprovalRequestAsync<TContext>(
        WorkflowApprovalRequestEvent @event,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : DbContext;
}

