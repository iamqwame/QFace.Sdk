namespace QimErp.Shared.Common.Services.Workflow;

public interface IWorkflowEntityApprovalHandlerRegistry
{
    IWorkflowEntityApprovalHandler? GetHandler(string entityType);
}

public sealed class WorkflowEntityApprovalHandlerRegistry : IWorkflowEntityApprovalHandlerRegistry
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly IReadOnlyDictionary<string, IWorkflowEntityApprovalHandler>? _handlers;

    public WorkflowEntityApprovalHandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal WorkflowEntityApprovalHandlerRegistry(IEnumerable<IWorkflowEntityApprovalHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.EntityType, StringComparer.OrdinalIgnoreCase);
    }

    public IWorkflowEntityApprovalHandler? GetHandler(string entityType)
    {
        if (_handlers is not null)
            return _handlers.TryGetValue(entityType, out var handler) ? handler : null;

        return _serviceProvider!
            .GetServices<IWorkflowEntityApprovalHandler>()
            .FirstOrDefault(h => h.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
    }
}
