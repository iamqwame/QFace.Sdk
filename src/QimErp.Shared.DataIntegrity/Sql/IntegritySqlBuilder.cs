using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using QimErp.Shared.DataIntegrity.Discovery;

namespace QimErp.Shared.DataIntegrity.Sql;

internal enum BatchRange
{
    None,
    UpTo,
    AfterAndUpTo
}

internal sealed partial class IntegritySqlBuilder(ISqlGenerationHelper sql)
{
    public const string SampleSizeParameter = "sample_size";
    public const string DeletedTargetParameter = "deleted_target";
    public const string DeletedRootParameter = "deleted_root";
    public const string AfterParameter = "after";
    public const string UpToParameter = "upto";
    public const string BatchSizeParameter = "batch_size";
    public const string SchemaParameter = "schema";
    public const string TableParameter = "table";
    public const string StatementTimeoutParameter = "statement_timeout_ms";
    public const string LockTimeoutParameter = "lock_timeout_ms";

    private const string UuidShape = "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";
    private const string IntegerShape = "-?[0-9]{1,18}";
    private const string UuidPattern = "'^" + UuidShape + "$'";
    private const string IntegerPattern = "'^" + IntegerShape + "$'";
    private const string EmptyUuid = "'00000000-0000-0000-0000-000000000000'::uuid";

    public string ReadOnly => "SET TRANSACTION READ ONLY";

    public string SessionLimits
        => $"SELECT set_config('statement_timeout', {P(StatementTimeoutParameter)}, true), set_config('lock_timeout', {P(LockTimeoutParameter)}, true)";

    public string RowEstimate
        => "SELECT c.reltuples::bigint FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace "
           + $"WHERE n.nspname = COALESCE({P(SchemaParameter)}, current_schema()) AND c.relname = {P(TableParameter)}";

    public string BatchUpperBound(IntegrityRoot root, bool hasLowerBound)
    {
        var key = Q(root.Key!.Column);
        var where = hasLowerBound ? $" WHERE r.{key} > {P(AfterParameter)}" : string.Empty;
        return $"SELECT b.k FROM (SELECT r.{key} AS k FROM {T(root.Table)} AS r{where} ORDER BY r.{key} LIMIT {P(BatchSizeParameter)}) AS b ORDER BY b.k DESC LIMIT 1";
    }

    public static bool IsUuidShaped(string value) => UuidRegex().IsMatch(value);

    public static bool IsSampleIdShaped(string value) => UuidRegex().IsMatch(value) || IntegerRegex().IsMatch(value);

    [GeneratedRegex("^" + UuidShape + @"\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UuidRegex();

    [GeneratedRegex("^" + IntegerShape + @"\z", RegexOptions.CultureInvariant)]
    private static partial Regex IntegerRegex();

    public string Link(IntegrityLink link, BatchRange range)
    {
        var root = link.Root;
        var target = link.Target;
        var column = $"{link.SourceAlias}.{Q(link.Column)}";
        var targetKey = $"t.{Q(target.Key!.Column)}";

        var kind = new StringBuilder();
        kind.Append($"CASE WHEN {targetKey} IS NULL THEN 'orphan'");
        if (link.Checks.Any(c => c.Kind == IntegrityFindingKind.TenantMismatch))
        {
            kind.Append($" WHEN t.{Q(target.TenantColumn!)} IS DISTINCT FROM r.{Q(root.TenantColumn!)}");
            if (target.IsGlobalColumn is not null)
                kind.Append($" AND t.{Q(target.IsGlobalColumn)} IS NOT TRUE");
            kind.Append(" THEN 'tenant_mismatch'");
        }

        if (link.Checks.Any(c => c.Kind == IntegrityFindingKind.DeletedTarget))
            kind.Append($" WHEN t.{Q(target.Status!.Column)} = {P(DeletedTargetParameter)} THEN 'deleted_target'");
        kind.Append(" END");

        var from = new StringBuilder();
        from.Append($"FROM {T(root.Table)} AS r");
        foreach (var join in link.Joins)
        {
            var on = string.Join(" AND ", join.ChildToParentColumns.Select(p => $"{join.Alias}.{Q(p.Key)} = {join.ParentAlias}.{Q(p.Value)}"));
            from.Append($"\n    JOIN {T(join.Table)} AS {join.Alias} ON {on}");
        }

        from.Append($"\n    LEFT JOIN {T(target.Table)} AS t ON {targetKey} = {Reference(column, link.ColumnFamily, target.Key.Family)}");
        from.Append($"\n    WHERE {Present(column, link.ColumnFamily)}{RootLive(root)}{Batch(root, range)}");

        if (root.Key is null)
        {
            return "SELECT k.kind, k.tenant_id, count(*) AS cnt, NULL::text AS sample_id\nFROM (\n"
                   + $"    SELECT {Tenant(root)} AS tenant_id,\n    {kind} AS kind\n    {from}\n"
                   + ") AS k\nWHERE k.kind IS NOT NULL\nGROUP BY k.kind, k.tenant_id\nORDER BY k.kind, k.tenant_id";
        }

        return "SELECT x.kind, x.tenant_id, x.cnt, x.sample_id\nFROM (\n"
               + "  SELECT k.kind, k.tenant_id, k.sample_id,\n"
               + "    count(*) OVER (PARTITION BY k.kind, k.tenant_id) AS cnt,\n"
               + "    dense_rank() OVER (PARTITION BY k.kind, k.tenant_id ORDER BY k.sample_key) AS rn\n"
               + "  FROM (\n"
               + $"    SELECT {Tenant(root)} AS tenant_id, r.{Q(root.Key.Column)} AS sample_key, {Sample(root)} AS sample_id,\n    {kind} AS kind\n    {from}\n"
               + "  ) AS k\n  WHERE k.kind IS NOT NULL\n"
               + $") AS x\nWHERE x.rn <= {P(SampleSizeParameter)}\nORDER BY x.kind, x.tenant_id, x.rn";
    }

    public string Unversioned(IntegrityVersionCheck check, BatchRange range)
    {
        var root = check.Root;
        var tenant = Tenant(root);
        var where = $"\n    WHERE (r.{Q(check.VersionColumn)} IS NULL OR r.{Q(check.VersionColumn)} <= 0){RootLive(root)}{Batch(root, range)}";

        if (root.Key is null)
        {
            return $"SELECT {tenant} AS tenant_id, count(*) AS cnt, NULL::text AS sample_id\nFROM {T(root.Table)} AS r{where}\n"
                   + $"GROUP BY {tenant}\nORDER BY 1";
        }

        return "SELECT x.tenant_id, x.cnt, x.sample_id\nFROM (\n"
               + $"    SELECT {tenant} AS tenant_id, {Sample(root)} AS sample_id,\n"
               + $"    count(*) OVER (PARTITION BY {tenant}) AS cnt,\n"
               + $"    row_number() OVER (PARTITION BY {tenant} ORDER BY r.{Q(root.Key.Column)}) AS rn\n"
               + $"    FROM {T(root.Table)} AS r{where}\n"
               + $") AS x\nWHERE x.rn <= {P(SampleSizeParameter)}\nORDER BY x.tenant_id, x.rn";
    }

    public string P(string name) => sql.GenerateParameterNamePlaceholder(name);

    private string Q(string identifier) => sql.DelimitIdentifier(identifier);

    private string T(IntegrityTableRef table) => sql.DelimitIdentifier(table.Name, table.Schema);

    private string Tenant(IntegrityRoot root)
        => root.TenantColumn is null ? "''::text" : $"COALESCE(r.{Q(root.TenantColumn)}, '')";

    private string Sample(IntegrityRoot root)
    {
        if (!root.Samples)
            return "NULL::text";

        var key = $"r.{Q(root.Key!.Column)}";
        return root.Key.Family == StoreFamily.Text
            ? $"CASE WHEN {key} ~* {UuidPattern} THEN {key} END"
            : $"{key}::text";
    }

    private string RootLive(IntegrityRoot root)
        => root.Status is null
            ? string.Empty
            : $" AND (r.{Q(root.Status.Column)} IS NULL OR r.{Q(root.Status.Column)} <> {P(DeletedRootParameter)})";

    private string Batch(IntegrityRoot root, BatchRange range)
    {
        if (range == BatchRange.None || root.Key is null)
            return string.Empty;

        var key = $"r.{Q(root.Key.Column)}";
        var upper = $" AND {key} <= {P(UpToParameter)}";
        return range == BatchRange.AfterAndUpTo ? $" AND {key} > {P(AfterParameter)}{upper}" : upper;
    }

    private static string Present(string column, StoreFamily family) => family switch
    {
        StoreFamily.Uuid => $"{column} IS NOT NULL AND {column} <> {EmptyUuid}",
        StoreFamily.Text => $"{column} IS NOT NULL AND btrim({column}) <> ''",
        _ => $"{column} IS NOT NULL AND {column} <> 0"
    };

    private static string Reference(string column, StoreFamily columnFamily, StoreFamily keyFamily)
    {
        if (columnFamily == keyFamily)
            return column;

        return (columnFamily, keyFamily) switch
        {
            (StoreFamily.Text, StoreFamily.Uuid) => $"CASE WHEN btrim({column}) ~* {UuidPattern} THEN btrim({column})::uuid END",
            (StoreFamily.Text, StoreFamily.Integer) => $"CASE WHEN btrim({column}) ~ {IntegerPattern} THEN btrim({column})::bigint END",
            (_, StoreFamily.Text) => $"{column}::text",
            _ => throw new InvalidOperationException($"No reference expression for {columnFamily} against a {keyFamily} key.")
        };
    }
}
