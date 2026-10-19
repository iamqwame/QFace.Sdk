namespace QimErp.Shared.DataIntegrity;

public enum IntegrityCheckStatus
{
    Clean,
    Findings,
    TimedOut,
    Failed,
    Skipped
}

/// <summary>Findings on a TimedOut or Failed outcome are partial and must not be treated as the check's result.</summary>
public sealed record IntegrityCheckOutcome(
    string Check,
    IntegrityFindingKind Kind,
    IntegrityCheckStatus Status,
    long Count,
    IReadOnlyList<IntegrityFinding> Findings,
    double DurationSeconds,
    string? FailureReason);
