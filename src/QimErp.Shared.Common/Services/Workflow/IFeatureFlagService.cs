namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Feature flags for workflow and orchestration (e.g. UseTemporalWorkflows).
/// </summary>
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagName, string? tenantId = null, CancellationToken cancellationToken = default);
}
