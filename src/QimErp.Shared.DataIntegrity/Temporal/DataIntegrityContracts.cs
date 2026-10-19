namespace QimErp.Shared.DataIntegrity.Temporal;

/// <summary>Deliberately has no TenantId: the run spans every tenant and must not be tenant-scoped by the activity interceptor.</summary>
public sealed record RunIntegrityChecksInput(string ModuleKey, int MaxRunMinutes);

/// <summary>Counts only; tenant ids and sample ids must never be added because this lands in Temporal history.</summary>
public sealed record DataIntegrityRunSummary(int Checks, int WithFindings, int Failed, int TimedOut, int Skipped, long TotalFindings);

/// <summary>Index of the next item to run; ContextIndex equal to the context count addresses the custom checks.</summary>
public sealed record DataIntegrityProgress(int ContextIndex, int LinkIndex);
