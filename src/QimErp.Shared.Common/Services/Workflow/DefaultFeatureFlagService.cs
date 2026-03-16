namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Reads feature flags from configuration (e.g. "FeatureFlags:UseTemporalWorkflows").
/// Returns false if not configured.
/// </summary>
public class DefaultFeatureFlagService(IConfiguration? configuration) : IFeatureFlagService
{
    public Task<bool> IsEnabledAsync(string flagName, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var key = $"FeatureFlags:{flagName}";
        var value = configuration?.GetSection(key).Value;
        if (string.IsNullOrEmpty(value))
            return Task.FromResult(false);
        return Task.FromResult(bool.TryParse(value, out var enabled) && enabled);
    }
}
