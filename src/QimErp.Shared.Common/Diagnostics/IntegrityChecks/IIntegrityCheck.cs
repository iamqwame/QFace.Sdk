namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// DATA-06 loose-link integrity check: counts orphans, tenant mismatches or local-copy drift
/// for one relationship. Registered via
/// <see cref="IntegrityCheckServiceCollectionExtensions.AddIntegrityCheck{TCheck}"/> and run by
/// <see cref="IntegrityCheckRunnerBackgroundService"/>.
/// </summary>
public interface IIntegrityCheck
{
    /// <summary>Stable identifier used in logs and metrics, e.g. "leave.request.employee_orphan".</summary>
    string Name { get; }

    Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken);
}
