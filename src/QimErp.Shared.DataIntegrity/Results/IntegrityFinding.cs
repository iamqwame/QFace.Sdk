namespace QimErp.Shared.DataIntegrity;

public enum IntegrityFindingKind
{
    Orphan,
    TenantMismatch,
    DeletedTarget,
    Unversioned,
    Custom
}

/// <summary>SampleIds holds root primary keys only (uuid, integer or Guid-shaped text), never reference values.</summary>
public sealed record IntegrityFinding(string Check, string TenantId, long Count, IReadOnlyList<string> SampleIds);
