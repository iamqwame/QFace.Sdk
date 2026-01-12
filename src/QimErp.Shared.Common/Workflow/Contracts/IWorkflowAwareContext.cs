namespace QimErp.Shared.Common.Workflow.Contracts;

public interface IWorkflowAwareContext
{
    // DbSet<WorkflowHistory> WorkflowHistories { get; }
    // DbSet<WorkflowTemplate> WorkflowTemplates { get; }
    // DbSet<WorkflowConfiguration> WorkflowConfigurations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
