using System.ComponentModel.DataAnnotations;

namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// Configures <see cref="IntegrityCheckRunnerBackgroundService"/>. Binds from the
/// "IntegrityCheckRunner" configuration section; falls back to these defaults when the
/// section is absent.
/// </summary>
public class IntegrityCheckRunnerOptions
{
    public const string SectionName = "IntegrityCheckRunner";

    /// <summary>DATA-06 calls for a nightly sweep; default is one run per day.</summary>
    [Range(1, 2_592_000, ErrorMessage = "PollingIntervalSeconds must be between 1 and 2,592,000 (30 days)")]
    public int PollingIntervalSeconds { get; set; } = 86_400;
}
