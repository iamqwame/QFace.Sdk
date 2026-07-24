namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Defines a contract for processing workflow return-for-edit requests.
/// </summary>
public interface IWorkflowReturnProcessor
{
    Task ProcessReturnRequestAsync<TContext>(
        WorkflowReturnRequestEvent @event,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : DbContext;
}
