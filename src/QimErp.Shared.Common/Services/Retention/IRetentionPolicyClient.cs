using QimErp.Shared.Common.Entities;

namespace QimErp.Shared.Common.Services.Retention;

public interface IRetentionPolicyClient
{
    /// <summary>Null = missing/inactive policy, Permanent, or lookup failure — callers must skip purge.</summary>
    Task<int?> GetRetentionDaysAfterCloseAsync(
        string tenantId,
        RetentionClass retentionClass,
        CancellationToken cancellationToken = default);
}
