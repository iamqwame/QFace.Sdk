namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// Outcome of one <see cref="IIntegrityCheck"/> run. Mirrors the DATA-06
/// <c>IntegrityRuns (RunAt, TenantId, Check, Count, SampleIds)</c> shape, minus RunAt/TenantId
/// which the caller attaches when it logs or persists the result.
/// </summary>
public sealed record IntegrityCheckResult(string CheckName, long Count, IReadOnlyList<string>? SampleIds = null)
{
    public bool HasFindings => Count > 0;

    public static IntegrityCheckResult Clean(string checkName) => new(checkName, 0);
}
