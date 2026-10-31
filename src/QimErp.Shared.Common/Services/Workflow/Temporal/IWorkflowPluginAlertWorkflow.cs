using Temporalio.Workflows;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

[Workflow("WorkflowPluginAlertWorkflow")]
public interface IWorkflowPluginAlertWorkflow
{
    [WorkflowRun]
    Task RunAsync(PluginAlertRequest request);
}

public sealed record PluginAlertRequest(
    string TenantId,
    string SourceEntityType,
    string SourceEntityId,
    string CorrelationId,
    string Title,
    string Body,
    string? ActionUrl,
    IReadOnlyList<string> Families,
    string? ProviderId,
    string? ResendOfHistoryId);
