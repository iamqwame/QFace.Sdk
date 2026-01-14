namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Defines a contract for processing workflow rejection requests.
/// </summary>
public interface IWorkflowRejectionProcessor
{
    /// <summary>
    /// Processes a workflow rejection request.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TContext"></typeparam>
    /// <returns></returns>
    Task ProcessRejectionRequestAsync<TContext>(
        WorkflowRejectionRequestEvent @event,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : DbContext;
}