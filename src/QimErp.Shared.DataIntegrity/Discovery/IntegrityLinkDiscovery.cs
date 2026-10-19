using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QimErp.Shared.DataIntegrity.Discovery;

internal static partial class IntegrityLinkDiscovery
{
    private const string ValueObjectSuffix = "ValueObject";

    private static readonly HashSet<string> SdkSources = new(StringComparer.Ordinal)
    {
        "OutboxMessage", "AppSetting", "Import", "EntityWorkflowStep", "EntityCodeConfig", "EntityCodeReservation", "TenantPluginFlag"
    };

    private static readonly HashSet<string> InfrastructureColumns = new(StringComparer.Ordinal)
    {
        "TenantId", "CompanyId", "CurrentWorkflowInstanceId", "CurrentWorkflowHistoryId", "WorkflowInstanceId",
        "WorkflowCompletedByEmployeeId", "WorkflowInitiatedByEmployeeId", "CorrelationId", "CausationId", "TraceId",
        "SpanId", "MessageId"
    };

    private static readonly HashSet<string> PolymorphicColumns = new(StringComparer.Ordinal)
    {
        "TargetId", "EntityId", "SubjectId", "ReferenceId", "RecordId", "SourceDocumentId", "SourceDocId", "SourceRecordId",
        "SourceEntityId", "ScopeEntityId", "SourceLineId", "TargetValueId", "LastTargetId"
    };

    private static readonly HashSet<string> RoleWords = new(StringComparer.Ordinal)
    {
        "Manager", "Supervisor", "Approver", "Reviewer", "Owner", "Assignee", "Requester", "Initiator", "Buddy", "Mentor",
        "Mentee", "Coach", "Appraiser", "Appraisee", "Interviewer", "Recruiter", "Signatory", "Delegate", "Reliever",
        "Nominee", "Nominator", "Successor", "Incumbent", "Person"
    };

    private static readonly HashSet<string> SelfQualifiers = new(StringComparer.Ordinal)
    {
        "Parent", "Source", "Original", "Previous", "Prior", "Next", "Matched", "Last", "Linked", "Related", "Superseded",
        "Renewed", "Converted", "Copied", "Cloned", "Reversal", "Reversed", "Merged", "Replaced", "Supervisor", "Reports"
    };

    private static readonly Dictionary<string, string> BuiltInAliases = new(StringComparer.Ordinal)
    {
        ["Account"] = "ChartOfAccount",
        ["GlAccount"] = "ChartOfAccount",
        ["Department"] = "OrganizationalUnit"
    };

    private static readonly HashSet<Type> IdClrTypes = [typeof(Guid), typeof(string), typeof(long), typeof(int), typeof(short)];

    private static readonly HashSet<Type> IntegralClrTypes = [typeof(long), typeof(int), typeof(short)];

    [GeneratedRegex(@"^(Created|LastModified|Modified|Updated|Deleted|Archived|Restored|EntityStatus\w*)By\w*Id$", RegexOptions.CultureInvariant)]
    private static partial Regex AuditPattern();

    [GeneratedRegex("FileId$", RegexOptions.CultureInvariant)]
    private static partial Regex BlobPattern();

    [GeneratedRegex(@"(External|Provider|Gateway|Remote|Temporal|Calendar|Timezone)\w*Id$", RegexOptions.CultureInvariant)]
    private static partial Regex ExternalPattern();

    [GeneratedRegex(@"Workflow\w*Id$", RegexOptions.CultureInvariant)]
    private static partial Regex WorkflowEnginePattern();

    public static IntegrityDiscoveryResult Discover(IModel model, DataIntegrityContextRegistration registration, string checkPrefix)
        => new Walker(model, registration, checkPrefix).Run();

    internal static StoreFamily? Family(string? storeType)
    {
        if (string.IsNullOrWhiteSpace(storeType))
            return null;

        var s = storeType.Trim().ToLowerInvariant();
        if (s == "uuid")
            return StoreFamily.Uuid;
        if (s is "text" or "citext" or "character" || s.StartsWith("character varying", StringComparison.Ordinal)
            || s.StartsWith("varchar", StringComparison.Ordinal) || s.StartsWith("character(", StringComparison.Ordinal))
            return StoreFamily.Text;
        if (s is "bigint" or "integer" or "smallint")
            return StoreFamily.Integer;
        return null;
    }

    private static string? NamePatternReason(string column)
    {
        if (AuditPattern().IsMatch(column))
            return "audit";
        if (BlobPattern().IsMatch(column))
            return "blob";
        if (ExternalPattern().IsMatch(column))
            return "external";
        if (WorkflowEnginePattern().IsMatch(column))
            return "workflow-engine";
        return null;
    }

    private static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    private static string? StoreType(IProperty property, in StoreObjectIdentifier storeObject)
        => property.FindColumn(storeObject)?.StoreType ?? property.GetColumnType(storeObject);

    private static IntegrityKeyColumn? KeyColumn(IProperty property, IntegrityTableRef table)
    {
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        var column = property.GetColumnName(storeObject);
        var family = Family(StoreType(property, storeObject));
        return column is null || family is null ? null : new IntegrityKeyColumn(column, family.Value);
    }

    private static string? NamedColumn(IEntityType entityType, string propertyName, IntegrityTableRef table, Func<Type, bool> clrTypeFilter)
    {
        var property = entityType.FindProperty(propertyName);
        if (property is null || !clrTypeFilter(property.ClrType))
            return null;
        return property.GetColumnName(StoreObjectIdentifier.Table(table.Name, table.Schema));
    }

    private static string? TenantColumn(IEntityType entityType, IntegrityTableRef table)
        => NamedColumn(entityType, "TenantId", table, t => t == typeof(string));

    private static string? IsGlobalColumn(IEntityType entityType, IntegrityTableRef table)
        => NamedColumn(entityType, "IsGlobal", table, t => Unwrap(t) == typeof(bool));

    private static IntegrityStatusColumn? StatusColumn(IEntityType entityType, IntegrityTableRef table)
        => StatusColumn(entityType, table, out _);

    private static IntegrityStatusColumn? StatusColumn(IEntityType entityType, IntegrityTableRef table, out (string Column, string StoreType)? unsupported)
    {
        unsupported = null;
        var property = entityType.FindProperty("DataStatus");
        if (property is null)
            return null;

        var enumType = Unwrap(property.ClrType);
        if (!enumType.IsEnum || !Enum.GetNames(enumType).Contains("Deleted", StringComparer.Ordinal))
            return null;

        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        var column = property.GetColumnName(storeObject);
        if (column is null)
            return null;

        var storeType = StoreType(property, storeObject);
        if (Family(storeType) is not (StoreFamily.Text or StoreFamily.Integer))
        {
            unsupported = (column, storeType ?? "unknown");
            return null;
        }

        var deleted = Enum.Parse(enumType, "Deleted");
        var converter = property.GetTypeMapping().Converter;
        var providerValue = converter is null
            ? Convert.ChangeType(deleted, Enum.GetUnderlyingType(enumType), System.Globalization.CultureInfo.InvariantCulture)
            : converter.ConvertToProvider(deleted);
        return providerValue is null ? null : new IntegrityStatusColumn(column, providerValue);
    }

    private static string? UnsupportedMapping(IEntityType entityType)
    {
        if (entityType.GetViewName() is not null || entityType.GetSqlQuery() is not null || entityType.GetFunctionName() is not null)
            return "view or query mapping";
        if (entityType.GetTableName() is null)
            return "no table mapping";
        var strategy = entityType.GetRootType().GetMappingStrategy();
        if (strategy is not null && strategy != RelationalAnnotationNames.TphMappingStrategy)
            return "unsupported mapping strategy";
        return null;
    }

    private static string DisplayName(IEntityType entityType)
        => entityType.HasSharedClrType ? entityType.ShortName() : entityType.ClrType.Name;

    private static string Canonical(IntegrityTableRef table, string column)
        => table.Schema is null ? $"{table.Name}.{column}" : $"{table.Schema}.{table.Name}.{column}";

    private sealed record CatalogueEntry(IEntityType EntityType, IntegrityTarget Target);

    private readonly record struct Lookup(CatalogueEntry? Target, IReadOnlyList<string>? Candidates)
    {
        public static readonly Lookup None = new(null, null);
    }

    private sealed record Resolution(
        IntegrityColumnStatus Status,
        CatalogueEntry? Target = null,
        string? Rule = null,
        string? Confidence = null,
        string? Reason = null,
        IReadOnlyList<string>? Candidates = null);

    private sealed class Walker
    {
        private readonly IModel _model;
        private readonly IRelationalModel _relational;
        private readonly DataIntegrityContextRegistration _registration;
        private readonly string _checkPrefix;
        private readonly Dictionary<string, List<CatalogueEntry>> _byShort = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CatalogueEntry> _byFull = new(StringComparer.Ordinal);
        private readonly Dictionary<IEntityType, CatalogueEntry> _byEntity = new();
        private readonly Dictionary<string, CatalogueEntry> _maps = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _excludes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CatalogueEntry> _aliases = new(StringComparer.Ordinal);
        private readonly HashSet<IntegrityTableRef> _noSampleTables = [];
        private readonly HashSet<string> _usedMaps = new(StringComparer.Ordinal);
        private readonly HashSet<string> _usedExcludes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _usedAliases = new(StringComparer.Ordinal);
        private readonly HashSet<string> _tableNamesInSeveralSchemas = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _seenColumns = new(StringComparer.Ordinal);
        private readonly HashSet<IntegrityTableRef> _reportedStatusTables = [];
        private readonly List<IntegrityUnusedOverride> _unusedOverrides = [];
        private readonly List<IntegrityColumnReport> _columns = [];
        private readonly List<IntegrityScan> _scans = [];

        public Walker(IModel model, DataIntegrityContextRegistration registration, string checkPrefix)
        {
            _model = model;
            _relational = model.GetRelationalModel();
            _registration = registration;
            _checkPrefix = checkPrefix;
        }

        public IntegrityDiscoveryResult Run()
        {
            BuildCatalogue();
            ValidateOverrides();

            foreach (var group in _relational.Tables.GroupBy(t => t.Name, StringComparer.Ordinal))
            {
                if (group.Select(t => t.Schema ?? string.Empty).Distinct(StringComparer.Ordinal).Count() > 1)
                    _tableNamesInSeveralSchemas.Add(group.Key);
            }

            foreach (var entityType in _model.GetEntityTypes().Where(e => !e.IsOwned()).OrderBy(e => e.Name, StringComparer.Ordinal))
                WalkRoot(entityType);

            CollectUnusedOverrides();

            var scans = _scans
                .OrderBy(s => s is IntegrityLink l ? l.SourceTable.Schema ?? string.Empty : s.Root.Table.Schema ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(s => s is IntegrityLink l ? l.SourceTable.Name : s.Root.Table.Name, StringComparer.Ordinal)
                .ThenBy(s => s.SortColumn, StringComparer.Ordinal)
                .ToList();

            var columns = _columns
                .OrderBy(c => c.Table.Schema ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(c => c.Table.Name, StringComparer.Ordinal)
                .ThenBy(c => c.Column, StringComparer.Ordinal)
                .ThenBy(c => c.Path, StringComparer.Ordinal)
                .ToList();

            return new IntegrityDiscoveryResult(
                _registration.ContextType,
                IntegrityNaming.ContextKey(_registration.ContextType),
                scans,
                columns,
                _unusedOverrides);
        }

        private void BuildCatalogue()
        {
            foreach (var entityType in _model.GetEntityTypes().OrderBy(e => e.Name, StringComparer.Ordinal))
            {
                if (entityType.IsOwned() || UnsupportedMapping(entityType) is not null)
                    continue;

                var primaryKey = entityType.GetRootType().FindPrimaryKey();
                if (primaryKey is null || primaryKey.Properties.Count != 1)
                    continue;

                var table = new IntegrityTableRef(entityType.GetTableName()!, entityType.GetSchema());
                var target = new IntegrityTarget(
                    entityType.ClrType.Name,
                    entityType.ClrType.FullName ?? entityType.ClrType.Name,
                    table,
                    KeyColumn(primaryKey.Properties[0], table),
                    TenantColumn(entityType, table),
                    IsGlobalColumn(entityType, table),
                    StatusColumn(entityType, table));

                var entry = new CatalogueEntry(entityType, target);
                _byEntity[entityType] = entry;
                _byFull[target.EntityFullName] = entry;
                if (!_byShort.TryGetValue(target.EntityName, out var list))
                    _byShort[target.EntityName] = list = [];
                list.Add(entry);
            }
        }

        private void ValidateOverrides()
        {
            var problems = new List<string>();
            var contextName = _registration.ContextType.Name;
            var links = _registration.Links;

            foreach (var (key, target) in links.Maps)
            {
                var canonical = ResolveKey(key);
                var entry = ResolveTarget(target, out var targetProblem);
                if (canonical is null)
                    problems.Add($"{contextName}: Map key '{key}' does not name a table.column in the model.");
                if (targetProblem is not null)
                    problems.Add($"{contextName}: Map target '{target}' for '{key}' {targetProblem}");
                if (canonical is null || entry is null)
                    continue;
                if (!_maps.TryAdd(canonical, entry))
                    problems.Add($"{contextName}: Map key '{key}' is declared more than once.");
            }

            foreach (var (key, reason) in links.Excludes)
            {
                var canonical = ResolveKey(key);
                if (canonical is null)
                {
                    problems.Add($"{contextName}: Exclude key '{key}' does not name a table.column in the model.");
                    continue;
                }

                if (!_excludes.TryAdd(canonical, reason))
                    problems.Add($"{contextName}: Exclude key '{key}' is declared more than once.");
            }

            foreach (var (stem, target) in links.Aliases)
            {
                var entry = ResolveTarget(target, out var targetProblem);
                if (targetProblem is not null)
                    problems.Add($"{contextName}: Alias target '{target}' for '{stem}' {targetProblem}");
                if (entry is null)
                    continue;
                if (!_aliases.TryAdd(stem, entry))
                    problems.Add($"{contextName}: Alias '{stem}' is declared more than once.");
            }

            foreach (var key in links.NoSampleTables)
            {
                var parts = key.Split('.');
                var table = parts.Length is 1 or 2 && !parts.Any(string.IsNullOrWhiteSpace)
                    ? ResolveTable(parts.Length == 2 ? parts[0] : null, parts[^1])
                    : null;
                if (table is null)
                {
                    problems.Add($"{contextName}: NoSamples table '{key}' does not name a table in the model.");
                    continue;
                }

                if (!_noSampleTables.Add(new IntegrityTableRef(table.Name, table.Schema)))
                    problems.Add($"{contextName}: NoSamples table '{key}' is declared more than once.");
            }

            if (problems.Count > 0)
                throw new DataIntegrityConfigurationException(problems);
        }

        private void CollectUnusedOverrides()
        {
            var contextName = _registration.ContextType.Name;
            var links = _registration.Links;

            foreach (var (key, target) in links.Maps)
            {
                if (!_usedMaps.Contains(ResolveKey(key)!))
                    _unusedOverrides.Add(new($"Map:{key}", $"{contextName}: Map '{key}' -> '{target}' was not applied; the column is not a link candidate or is excluded first."));
            }

            foreach (var (key, _) in links.Excludes)
            {
                if (!_usedExcludes.Contains(ResolveKey(key)!))
                    _unusedOverrides.Add(new($"Exclude:{key}", $"{contextName}: Exclude key '{key}' was not applied; the column is not a link candidate or is excluded by a built-in rule first."));
            }

            foreach (var (stem, target) in links.Aliases)
            {
                if (!_usedAliases.Contains(stem))
                    _unusedOverrides.Add(new($"Alias:{stem}", $"{contextName}: Alias '{stem}' -> '{target}' was not applied by any stem lookup."));
            }
        }

        private ITable? ResolveTable(string? schema, string tableName)
        {
            var tables = _relational.Tables
                .Where(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase)
                            && (schema is null || string.Equals(t.Schema, schema, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return tables.Count == 1
                ? tables[0]
                : tables.SingleOrDefault(t => t.Schema is null);
        }

        private string? ResolveKey(string key)
        {
            var parts = key.Split('.');
            if (parts.Length is < 2 or > 3 || parts.Any(string.IsNullOrWhiteSpace))
                return null;

            var table = ResolveTable(parts.Length == 3 ? parts[0] : null, parts[^2]);
            if (table is null)
                return null;

            var columnName = parts[^1];
            var column = table.Columns.FirstOrDefault(c => string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase));
            return column is null ? null : Canonical(new IntegrityTableRef(table.Name, table.Schema), column.Name);
        }

        private CatalogueEntry? ResolveTarget(string name, out string? problem)
        {
            problem = null;
            if (_byFull.TryGetValue(name, out var full))
                return full;
            if (_byShort.TryGetValue(name, out var list))
            {
                if (list.Count == 1)
                    return list[0];
                problem = "is ambiguous; use the full CLR name (" + string.Join(", ", list.Select(e => e.Target.EntityFullName).Order(StringComparer.Ordinal)) + ").";
                return null;
            }

            problem = "is not an entity with a table and a single-column primary key in the model.";
            return null;
        }

        private void WalkRoot(IEntityType entityType)
        {
            var name = DisplayName(entityType);
            var tableName = entityType.GetTableName();
            var table = new IntegrityTableRef(tableName ?? name, tableName is null ? null : entityType.GetSchema());

            string? reason = null;
            if (SdkSources.Contains(entityType.ClrType.Name))
                reason = "sdk-infrastructure";
            else if (entityType.FindPrimaryKey() is null)
                reason = "keyless";
            else
                reason = UnsupportedMapping(entityType);

            if (reason is not null)
            {
                AddEntityExclusion(table, name, reason);
                return;
            }

            var root = BuildRoot(entityType, table, name);
            WalkType(entityType, entityType, root, table, "r", [], name);
            AddVersionCheck(entityType, root);
        }

        private IntegrityRoot BuildRoot(IEntityType entityType, IntegrityTableRef table, string name)
        {
            var primaryKey = entityType.FindPrimaryKey()!;
            var key = primaryKey.Properties.Count == 1 ? KeyColumn(primaryKey.Properties[0], table) : null;
            var status = StatusColumn(entityType, table, out var unsupported);
            if (unsupported is { } u && _reportedStatusTables.Add(table))
            {
                _columns.Add(new IntegrityColumnReport(
                    table, u.Column, $"{name}.DataStatus", IntegrityColumnStatus.Excluded, null, null, null,
                    $"DataStatus store type {u.StoreType} unsupported: deleted filters disabled", IndexCoverage.None, [], null));
            }

            return new IntegrityRoot(table, name, key, TenantColumn(entityType, table), status, !_noSampleTables.Contains(table));
        }

        private void AddEntityExclusion(IntegrityTableRef table, string path, string reason)
            => _columns.Add(new IntegrityColumnReport(table, "*", path, IntegrityColumnStatus.Excluded, null, null, null, reason, IndexCoverage.None, [], null));

        private void AddVersionCheck(IEntityType entityType, IntegrityRoot root)
        {
            var property = entityType.FindDeclaredProperty("SourceVersion");
            if (property is null || property.IsShadowProperty() || !IntegralClrTypes.Contains(Unwrap(property.ClrType)))
                return;

            var column = property.GetColumnName(StoreObjectIdentifier.Table(root.Table.Name, root.Table.Schema));
            if (column is null)
                return;

            var name = $"{_checkPrefix}.{TableSegment(root.Table)}.source_version_unversioned";
            _scans.Add(new IntegrityVersionCheck(
                $"{_checkPrefix}.{TableSegment(root.Table)}.source_version",
                root,
                [new IntegrityCheckName(IntegrityFindingKind.Unversioned, name)],
                column));
        }

        private string TableSegment(IntegrityTableRef table)
            => _tableNamesInSeveralSchemas.Contains(table.Name) && table.Schema is not null
                ? IntegrityNaming.Snake(table.Schema + "_" + table.Name)
                : IntegrityNaming.Snake(table.Name);

        private void WalkType(
            IEntityType rootEntity,
            IEntityType type,
            IntegrityRoot root,
            IntegrityTableRef table,
            string alias,
            IReadOnlyList<IntegrityOwnerJoin> joins,
            string path)
        {
            var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
            var primaryKey = type.FindPrimaryKey()?.Properties ?? [];
            var isValueObject = type.IsOwned() && type.ClrType.Name.EndsWith(ValueObjectSuffix, StringComparison.Ordinal)
                                && type.ClrType.Name.Length > ValueObjectSuffix.Length;

            foreach (var property in type.GetDeclaredProperties().OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                var column = property.GetColumnName(storeObject);
                if (column is null)
                    continue;

                var isVoCandidate = isValueObject && property.Name == "Id";
                var isNameCandidate = column.Length > 2 && column.EndsWith("Id", StringComparison.Ordinal);
                if (!isVoCandidate && !isNameCandidate)
                    continue;

                var propertyPath = $"{path}.{property.Name}";
                var canonical = Canonical(table, column);
                var index = Coverage(table, column);

                if (_seenColumns.TryGetValue(canonical, out var firstPath))
                {
                    Report(table, column, propertyPath, new Resolution(IntegrityColumnStatus.Excluded, Reason: $"shared column; covered by {firstPath}"), index, null);
                    continue;
                }

                _seenColumns[canonical] = propertyPath;

                var skip = PropertySkipReason(property, primaryKey, storeObject);
                if (skip is not null)
                {
                    Report(table, column, propertyPath, new Resolution(IntegrityColumnStatus.Excluded, Reason: skip), index, null);
                    continue;
                }

                var family = Family(StoreType(property, storeObject))!.Value;
                var resolution = Exclusion(canonical, column, isVoCandidate)
                                 ?? Resolve(rootEntity, type, canonical, column, isVoCandidate);
                resolution = CheckCompatibility(resolution, family);

                IntegrityLink? link = null;
                if (resolution.Status == IntegrityColumnStatus.Checked)
                {
                    link = BuildLink(root, joins, table, alias, column, family, resolution.Target!.Target);
                    _scans.Add(link);
                }

                Report(table, column, propertyPath, resolution, index, link);
            }

            foreach (var navigation in type.GetDeclaredNavigations()
                         .Where(n => n.ForeignKey.IsOwnership && !n.IsOnDependent)
                         .OrderBy(n => n.Name, StringComparer.Ordinal))
            {
                var owned = navigation.TargetEntityType;
                var ownedPath = $"{path}.{navigation.Name}";

                if (owned.ClrType.Name == "EntityReference")
                {
                    AddEntityExclusion(table, ownedPath, "polymorphic");
                    continue;
                }

                if (owned.IsMappedToJson())
                {
                    AddEntityExclusion(table, ownedPath, "json");
                    continue;
                }

                var ownedTableName = owned.GetTableName();
                if (ownedTableName is null || owned.GetViewName() is not null || owned.GetSqlQuery() is not null)
                {
                    AddEntityExclusion(table, ownedPath, "view or query mapping");
                    continue;
                }

                var ownedTable = new IntegrityTableRef(ownedTableName, owned.GetSchema());
                if (ownedTable == table)
                {
                    WalkType(rootEntity, owned, root, table, alias, joins, ownedPath);
                    continue;
                }

                var ownedStore = StoreObjectIdentifier.Table(ownedTable.Name, ownedTable.Schema);
                var foreignKey = navigation.ForeignKey;
                var pairs = new List<KeyValuePair<string, string>>();
                for (var i = 0; i < foreignKey.Properties.Count; i++)
                {
                    var child = foreignKey.Properties[i].GetColumnName(ownedStore);
                    var parent = foreignKey.PrincipalKey.Properties[i].GetColumnName(storeObject);
                    if (child is null || parent is null)
                    {
                        pairs.Clear();
                        break;
                    }

                    pairs.Add(new(child, parent));
                }

                if (pairs.Count == 0)
                {
                    AddEntityExclusion(ownedTable, ownedPath, "ownership columns not mapped");
                    continue;
                }

                var ownedAlias = $"s{joins.Count + 1}";
                var ownedJoins = joins.Append(new IntegrityOwnerJoin(ownedTable, ownedAlias, alias, pairs)).ToList();
                WalkType(rootEntity, owned, root, ownedTable, ownedAlias, ownedJoins, ownedPath);
            }
        }

        private static string? PropertySkipReason(IProperty property, IReadOnlyList<IProperty> primaryKey, in StoreObjectIdentifier storeObject)
        {
            if (primaryKey.Contains(property))
                return "primary key";
            if (property.IsForeignKey())
                return "foreign key";
            if (property.IsShadowProperty())
                return "shadow property";

            var storeType = StoreType(property, storeObject);
            if (storeType is not null && storeType.TrimEnd().EndsWith("[]", StringComparison.Ordinal))
                return "array";

            var providerType = Unwrap(property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType);
            if (!IdClrTypes.Contains(providerType))
                return "unsupported type";
            if (Family(storeType) is null)
                return "unsupported store type";
            return null;
        }

        private Resolution? Exclusion(string canonical, string column, bool isVoCandidate)
        {
            if (_excludes.TryGetValue(canonical, out var reason))
            {
                _usedExcludes.Add(canonical);
                return new Resolution(IntegrityColumnStatus.Excluded, Rule: "override", Reason: reason);
            }

            if (InfrastructureColumns.Contains(column))
                return new Resolution(IntegrityColumnStatus.Excluded, Reason: "infrastructure");
            if (isVoCandidate)
                return null;

            var pattern = NamePatternReason(column);
            if (pattern is not null)
                return new Resolution(IntegrityColumnStatus.Excluded, Reason: pattern);
            if (PolymorphicColumns.Contains(column))
                return new Resolution(IntegrityColumnStatus.Excluded, Reason: "polymorphic");
            return null;
        }

        private Resolution Resolve(IEntityType rootEntity, IEntityType sourceType, string canonical, string column, bool isVoCandidate)
        {
            if (_maps.TryGetValue(canonical, out var mapped))
            {
                _usedMaps.Add(canonical);
                return new Resolution(IntegrityColumnStatus.Checked, mapped, "override", "high");
            }

            if (isVoCandidate)
            {
                var stem = sourceType.ClrType.Name[..^ValueObjectSuffix.Length];
                var lookup = Find(stem);
                if (lookup.Candidates is not null)
                    return Ambiguous(lookup.Candidates);
                return lookup.Target is not null
                    ? new Resolution(IntegrityColumnStatus.Checked, lookup.Target, "value-object", "high")
                    : new Resolution(IntegrityColumnStatus.Unverifiable, Reason: $"no local copy of {stem}");
            }

            var stemName = column[..^2];
            var words = IntegrityNaming.Words(stemName);
            var rootName = rootEntity.ClrType.Name;
            _byEntity.TryGetValue(rootEntity, out var ownRoot);

            if (stemName == "Parent" && ownRoot is not null)
                return new Resolution(IntegrityColumnStatus.Checked, ownRoot, "parent", "high");

            if (words.Count > 1 && words[0] == "Parent")
            {
                var rest = string.Concat(words.Skip(1));
                var lookup = Find(rest);
                if (lookup.Candidates is not null)
                    return Ambiguous(lookup.Candidates);
                if (lookup.Target is not null && lookup.Target != ownRoot)
                    return new Resolution(IntegrityColumnStatus.Checked, lookup.Target, "parent", "high");
                if (ownRoot is not null && rootName.EndsWith(rest, StringComparison.Ordinal))
                    return new Resolution(IntegrityColumnStatus.Checked, ownRoot, "parent", "high");
            }

            for (var i = 0; i < words.Count; i++)
            {
                var lookup = Find(string.Concat(words.Skip(i)));
                if (lookup.Candidates is not null)
                    return Ambiguous(lookup.Candidates);
                if (lookup.Target is null)
                    continue;

                var resolution = i == 0
                    ? new Resolution(IntegrityColumnStatus.Checked, lookup.Target, "exact", "high")
                    : new Resolution(IntegrityColumnStatus.Checked, lookup.Target, "suffix", "medium");
                return SelfNamed(resolution, rootEntity, words);
            }

            if (_byShort.TryGetValue("Employee", out var employees)
                && (RoleWords.Contains(stemName) || (words.Count > 0 && RoleWords.Contains(words[^1])) || stemName.EndsWith("By", StringComparison.Ordinal)))
            {
                return employees.Count == 1
                    ? new Resolution(IntegrityColumnStatus.Checked, employees[0], "role-alias", "medium")
                    : Ambiguous(employees.Select(e => e.Target.EntityFullName).ToList());
            }

            var stems = words.Count > 1 ? new[] { stemName, string.Concat(words.Skip(1)) } : new[] { stemName };
            foreach (var stem in stems)
            {
                var stemWords = IntegrityNaming.Words(stem);
                var candidates = _byShort.Keys
                    .Where(t => t != stem && EndsWithWords(IntegrityNaming.Words(t), stemWords))
                    .Order(StringComparer.Ordinal)
                    .ToList();
                if (candidates.Count == 0)
                    continue;

                if (candidates.Count > 1)
                {
                    var rootFirstWord = IntegrityNaming.Words(rootName).FirstOrDefault();
                    var sameFirstWord = candidates.Where(c => IntegrityNaming.Words(c).FirstOrDefault() == rootFirstWord).ToList();
                    if (sameFirstWord.Count == 1)
                        candidates = sameFirstWord;
                }

                if (candidates.Count > 1)
                    return Ambiguous(candidates);

                var entries = _byShort[candidates[0]];
                if (entries.Count > 1)
                    return Ambiguous(entries.Select(e => e.Target.EntityFullName).ToList());

                return SelfNamed(new Resolution(IntegrityColumnStatus.LowConfidence, entries[0], "ends-with", "low"), rootEntity, words);
            }

            return new Resolution(IntegrityColumnStatus.Unresolved, Reason: $"no entity for {stemName}");
        }

        private static bool EndsWithWords(IReadOnlyList<string> words, IReadOnlyList<string> suffix)
        {
            if (suffix.Count == 0 || words.Count <= suffix.Count)
                return false;
            var offset = words.Count - suffix.Count;
            for (var i = 0; i < suffix.Count; i++)
            {
                if (!string.Equals(words[offset + i], suffix[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static Resolution SelfNamed(Resolution resolution, IEntityType rootEntity, IReadOnlyList<string> words)
        {
            if (resolution.Target?.EntityType != rootEntity)
                return resolution;
            if (words.Count > 0 && SelfQualifiers.Contains(words[0]))
                return resolution;
            return new Resolution(IntegrityColumnStatus.Excluded, resolution.Target, resolution.Rule, resolution.Confidence, "self-named key");
        }

        private static Resolution Ambiguous(IReadOnlyList<string> candidates)
            => new(IntegrityColumnStatus.Ambiguous, Candidates: candidates.Order(StringComparer.Ordinal).ToList());

        private static Resolution CheckCompatibility(Resolution resolution, StoreFamily columnFamily)
        {
            if (resolution.Status is not (IntegrityColumnStatus.Checked or IntegrityColumnStatus.LowConfidence) || resolution.Target is null)
                return resolution;

            var key = resolution.Target.Target.Key;
            var incompatible = key is null
                               || (columnFamily == StoreFamily.Uuid && key.Family == StoreFamily.Integer)
                               || (columnFamily == StoreFamily.Integer && key.Family == StoreFamily.Uuid);
            return incompatible
                ? resolution with { Status = IntegrityColumnStatus.Excluded, Reason = "type-incompatible" }
                : resolution;
        }

        private Lookup Find(string name)
        {
            foreach (var form in Forms(name))
            {
                if (_byShort.TryGetValue(form, out var entries))
                    return FromEntries(entries);
                if (_aliases.TryGetValue(form, out var alias))
                {
                    _usedAliases.Add(form);
                    return new Lookup(alias, null);
                }

                if (BuiltInAliases.TryGetValue(form, out var builtIn) && _byShort.TryGetValue(builtIn, out var builtInEntries))
                    return FromEntries(builtInEntries);
            }

            return Lookup.None;
        }

        private static Lookup FromEntries(List<CatalogueEntry> entries)
            => entries.Count == 1
                ? new Lookup(entries[0], null)
                : new Lookup(null, entries.Select(e => e.Target.EntityFullName).ToList());

        private static IEnumerable<string> Forms(string name)
        {
            if (name.Length == 0)
                yield break;
            yield return name;
            if (name.Length > 1 && name.EndsWith('s'))
                yield return name[..^1];
            if (name.Length > 2 && name.EndsWith("es", StringComparison.Ordinal))
                yield return name[..^2];
        }

        private IndexCoverage Coverage(IntegrityTableRef table, string column)
        {
            var relationalTable = _relational.FindTable(table.Name, table.Schema);
            if (relationalTable is null)
                return IndexCoverage.None;

            var columnSets = relationalTable.Indexes.Select(i => i.Columns.Select(c => c.Name).ToList())
                .Concat(relationalTable.UniqueConstraints.Select(u => u.Columns.Select(c => c.Name).ToList()));
            if (relationalTable.PrimaryKey is not null)
                columnSets = columnSets.Append(relationalTable.PrimaryKey.Columns.Select(c => c.Name).ToList());

            var coverage = IndexCoverage.None;
            foreach (var set in columnSets)
            {
                if (set.Count > 0 && set[0] == column)
                    return IndexCoverage.Leading;
                if (set.Contains(column))
                    coverage = IndexCoverage.NonLeading;
            }

            return coverage;
        }

        private IntegrityLink BuildLink(
            IntegrityRoot root,
            IReadOnlyList<IntegrityOwnerJoin> joins,
            IntegrityTableRef table,
            string alias,
            string column,
            StoreFamily family,
            IntegrityTarget target)
        {
            var prefix = $"{_checkPrefix}.{TableSegment(table)}.{IntegrityNaming.Snake(column)}";
            var checks = new List<IntegrityCheckName> { new(IntegrityFindingKind.Orphan, prefix + "_orphan") };
            if (root.TenantColumn is not null && target.TenantColumn is not null)
                checks.Add(new(IntegrityFindingKind.TenantMismatch, prefix + "_tenant_mismatch"));
            if (target.Status is not null)
                checks.Add(new(IntegrityFindingKind.DeletedTarget, prefix + "_deleted_target"));

            return new IntegrityLink(prefix, root, checks, joins, table, alias, column, family, target);
        }

        private void Report(IntegrityTableRef table, string column, string path, Resolution resolution, IndexCoverage index, IntegrityLink? link)
            => _columns.Add(new IntegrityColumnReport(
                table,
                column,
                path,
                resolution.Status,
                resolution.Target?.Target.EntityName,
                resolution.Rule,
                resolution.Confidence,
                resolution.Reason,
                index,
                resolution.Candidates ?? [],
                link));
    }
}
