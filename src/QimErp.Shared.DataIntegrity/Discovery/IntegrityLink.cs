namespace QimErp.Shared.DataIntegrity.Discovery;

internal enum StoreFamily
{
    Uuid,
    Text,
    Integer
}

internal enum IndexCoverage
{
    Leading,
    NonLeading,
    None
}

internal enum IntegrityColumnStatus
{
    Checked,
    LowConfidence,
    Excluded,
    Unverifiable,
    Unresolved,
    Ambiguous
}

internal sealed record IntegrityTableRef(string Name, string? Schema)
{
    public string Display => Schema is null ? Name : $"{Schema}.{Name}";
}

internal sealed record IntegrityKeyColumn(string Column, StoreFamily Family);

internal sealed record IntegrityStatusColumn(string Column, object DeletedValue);

internal sealed record IntegrityRoot(
    IntegrityTableRef Table,
    string EntityName,
    IntegrityKeyColumn? Key,
    string? TenantColumn,
    IntegrityStatusColumn? Status,
    bool Samples);

internal sealed record IntegrityOwnerJoin(
    IntegrityTableRef Table,
    string Alias,
    string ParentAlias,
    IReadOnlyList<KeyValuePair<string, string>> ChildToParentColumns);

internal sealed record IntegrityTarget(
    string EntityName,
    string EntityFullName,
    IntegrityTableRef Table,
    IntegrityKeyColumn? Key,
    string? TenantColumn,
    string? IsGlobalColumn,
    IntegrityStatusColumn? Status);

internal sealed record IntegrityCheckName(IntegrityFindingKind Kind, string Name);

internal abstract record IntegrityScan(string CheckPrefix, IntegrityRoot Root, IReadOnlyList<IntegrityCheckName> Checks)
{
    public abstract string SortColumn { get; }
}

internal sealed record IntegrityLink(
    string CheckPrefix,
    IntegrityRoot Root,
    IReadOnlyList<IntegrityCheckName> Checks,
    IReadOnlyList<IntegrityOwnerJoin> Joins,
    IntegrityTableRef SourceTable,
    string SourceAlias,
    string Column,
    StoreFamily ColumnFamily,
    IntegrityTarget Target) : IntegrityScan(CheckPrefix, Root, Checks)
{
    public override string SortColumn => Column;
}

internal sealed record IntegrityVersionCheck(
    string CheckPrefix,
    IntegrityRoot Root,
    IReadOnlyList<IntegrityCheckName> Checks,
    string VersionColumn) : IntegrityScan(CheckPrefix, Root, Checks)
{
    public override string SortColumn => VersionColumn;
}

internal sealed record IntegrityColumnReport(
    IntegrityTableRef Table,
    string Column,
    string Path,
    IntegrityColumnStatus Status,
    string? Target,
    string? Rule,
    string? Confidence,
    string? Reason,
    IndexCoverage Index,
    IReadOnlyList<string> Candidates,
    IntegrityLink? Link);

internal sealed record IntegrityDiscoveryResult(
    Type ContextType,
    string ContextKey,
    IReadOnlyList<IntegrityScan> Scans,
    IReadOnlyList<IntegrityColumnReport> Columns)
{
    public IEnumerable<IntegrityColumnReport> With(IntegrityColumnStatus status) => Columns.Where(c => c.Status == status);

    public int CheckedLinks => With(IntegrityColumnStatus.Checked).Count();

    public int Unindexed => With(IntegrityColumnStatus.Checked).Count(c => c.Index == IndexCoverage.None);

    public int Unresolved => Columns.Count(c => c.Status is IntegrityColumnStatus.Unresolved or IntegrityColumnStatus.Unverifiable);

    public int Ambiguous => With(IntegrityColumnStatus.Ambiguous).Count();

    public int Excluded => Columns.Count(c => c.Status is IntegrityColumnStatus.Excluded or IntegrityColumnStatus.LowConfidence);

    public int CheckCount => Scans.Sum(s => s.Checks.Count);
}
