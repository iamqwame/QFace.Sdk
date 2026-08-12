using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services;

/// <summary>
/// Abstract base implementation of IEntityCodeService.
/// Modules create a thin concrete subclass that supplies the DbContext.
/// TContext must expose an EntityCodeConfigs table (either via a typed DbSet property
/// or simply by having the entity registered in the model — Set&lt;T&gt;() is used internally).
///
/// Concurrency contract:
///   Single allocation  → UPDATE … SET LastSequence = LastSequence + 1 RETURNING LastSequence
///   Batch allocation   → UPDATE … SET LastSequence = LastSequence + N RETURNING LastSequence
///   Both are atomic at the DB level — no application-level lock needed.
///
/// Caching:
///   The EntityCodeConfig row (format + mode + reset metadata) is hit on every
///   entity-create across every module. Reads go through a tenant-scoped Redis
///   key (1h TTL) and writes (UpsertConfigAsync, ReconcileManualToAutoAsync,
///   CheckAndApplyResetAsync, GetOrCreateConfigAsync auto-create) invalidate
///   it inline. The hot path GenerateBatchAsync still uses an atomic raw-SQL
///   UPDATE … RETURNING for LastSequence — the cache only short-circuits the
///   format-and-mode lookup, never the sequence allocation itself.
/// </summary>
public abstract class EntityCodeService<TContext> : IEntityCodeService
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly IDistributedCacheService _cache;
    protected readonly ILogger _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private static string CacheKey(string tenantId, string entityType)
        => $"shared:{tenantId}:lookup:entity-code-config:{entityType}";

    // Every entity type a module knows about is declared explicitly via the moduleDefaults
    // constructor parameter in that module's own concrete subclass — there is no implicit
    // SDK-wide set shared across all modules. A module's numbering page only ever lists
    // entity types it actually generates codes for.
    private readonly IReadOnlyDictionary<string, (string Prefix, string Separator, bool IncludeYear, int PaddingWidth)> _defaults;
    private readonly string _moduleName;

    /// <param name="moduleName">
    /// Display name of the module this service belongs to (e.g. "Payroll", "Inventory").
    /// Attributed to every entity type in <paramref name="moduleDefaults"/> — this is the only
    /// module whose settings page will ever list these entity types.
    /// </param>
    /// <param name="moduleDefaults">
    /// Entity types this module generates codes for. A module that shares an entity type with
    /// another module (e.g. AR and Billing both generate "Invoice" numbers) registers its own
    /// entry here — each module keeps a fully separate config/sequence in its own database.
    /// </param>
    protected EntityCodeService(
        TContext context,
        IDistributedCacheService cache,
        ILogger logger,
        string moduleName,
        IReadOnlyDictionary<string, (string Prefix, string Separator, bool IncludeYear, int PaddingWidth)>? moduleDefaults = null)
    {
        _context = context;
        _cache   = cache;
        _logger  = logger;
        _moduleName = moduleName;
        _defaults = moduleDefaults ?? new Dictionary<string, (string, string, bool, int)>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> GenerateAsync(string tenantId, string entityType, CancellationToken ct = default)
    {
        var codes = await GenerateBatchAsync(tenantId, entityType, 1, ct);
        return codes[0];
    }

    public async Task<string[]> GenerateBatchAsync(string tenantId, string entityType, int count, CancellationToken ct = default)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be ≥ 1.");

        var config = await GetOrCreateConfigAsync(tenantId, entityType, ct);
        await CheckAndApplyResetAsync(config, ct);

        // Atomic increment: UPDATE … SET LastSequence = LastSequence + @count RETURNING LastSequence
        // The returned value is the FINAL LastSequence (after adding count).
        // Codes use values [final - count + 1 … final].
        var finalSeq = await IncrementSequenceAsync(tenantId, entityType, count, ct);

        var firstSeq = finalSeq - count + 1;
        var now = DateTimeOffset.UtcNow;
        return Enumerable.Range(0, count)
            .Select(i => config.FormatCode(firstSeq + i, now))
            .ToArray();
    }

    public async Task<string> SuggestAsync(string tenantId, string entityType, CancellationToken ct = default)
    {
        // Advisory only — reads current LastSequence + 1 without incrementing.
        var config = await GetOrCreateConfigAsync(tenantId, entityType, ct);
        return config.FormatCode(config.LastSequence + 1, DateTimeOffset.UtcNow);
    }

    public async Task<EntityCodeConfig?> GetConfigAsync(string tenantId, string entityType, CancellationToken ct = default)
    {
        var key = CacheKey(tenantId, entityType);
        var cached = await _cache.GetAsync<EntityCodeConfig>(key);
        if (cached is not null) return cached;

        // IgnoreQueryFilters: the query is already explicitly tenant-scoped via TenantId == tenantId.
        // Without this, the global EF query filter (which reads from ITenantContext) returns no rows
        // when called from Temporal activities that run without an active audit context — causing
        // UpsertConfigAsync to attempt a duplicate INSERT and hit IX_EntityCodeConfigs_TenantId_EntityType.
        var fresh = await _context.Set<EntityCodeConfig>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.EntityType == entityType, ct);

        if (fresh is not null)
            await _cache.SetAsync(key, fresh, CacheTtl);
        return fresh;
    }

    public async Task UpsertConfigAsync(
        string tenantId, string entityType,
        string prefix, string separator, bool includeYear, int paddingWidth,
        CodeGenerationMode mode, CodeResetPeriod resetPeriod,
        CancellationToken ct = default)
    {
        // Bypass the cache here so the upsert path always sees the latest DB row;
        // otherwise we could redo an INSERT against a stale "not found" cache hit.
        var existing = await _context.Set<EntityCodeConfig>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.EntityType == entityType, ct);

        if (existing is null)
        {
            var config = EntityCodeConfig.Create(tenantId, entityType,
                prefix, separator, includeYear, paddingWidth, mode, resetPeriod);
            _context.Set<EntityCodeConfig>().Add(config);
        }
        else
        {
            existing.UpdateFormat(prefix, separator, includeYear, paddingWidth);
            existing.SetMode(mode);
            existing.SetResetPeriod(resetPeriod);
        }

        await _context.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey(tenantId, entityType));
    }

    public async Task<long> ReconcileManualToAutoAsync(
        string tenantId, string entityType,
        IEnumerable<string> existingCodes, CancellationToken ct = default)
    {
        var maxFound = existingCodes
            .Select(code => ExtractNumericSuffix(code))
            .Where(n => n.HasValue)
            .Select(n => n!.Value)
            .DefaultIfEmpty(0)
            .Max();

        var config = await GetOrCreateConfigAsync(tenantId, entityType, ct);
        config.SetManualHighWaterMark(maxFound);
        config.SetMode(CodeGenerationMode.Auto);
        await _context.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey(tenantId, entityType));

        _logger.LogInformation(
            "Reconciled manual→auto for {EntityType} on tenant {TenantId}: high-water={HWM}",
            entityType, tenantId, maxFound);

        return maxFound;
    }

    public IReadOnlyCollection<string> GetKnownEntityTypes() => _defaults.Keys.ToArray();

    public string GetModuleFor(string entityType) => _moduleName;

    public async Task<IReadOnlyList<EntityCodeConfig>> GetAllConfigsAsync(string tenantId, CancellationToken ct = default)
    {
        var configs = new List<EntityCodeConfig>(_defaults.Count);
        foreach (var entityType in _defaults.Keys)
        {
            configs.Add(await GetOrCreateConfigAsync(tenantId, entityType, ct));
        }
        return configs;
    }

    private async Task<EntityCodeConfig> GetOrCreateConfigAsync(
        string tenantId, string entityType, CancellationToken ct)
    {
        var config = await GetConfigAsync(tenantId, entityType, ct);
        if (config is not null) return config;

        // Auto-create from merged defaults (SDK + module); bare minimum if entity type is unknown
        _defaults.TryGetValue(entityType, out var def);
        config = EntityCodeConfig.Create(
            tenantId, entityType,
            prefix:      def.Prefix      ?? entityType[..Math.Min(3, entityType.Length)].ToUpper(),
            separator:   def.Separator   ?? "-",
            includeYear: def.IncludeYear,
            paddingWidth: def.PaddingWidth > 0 ? def.PaddingWidth : 4);

        _context.Set<EntityCodeConfig>().Add(config);
        await _context.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey(tenantId, entityType));

        _logger.LogInformation(
            "Auto-created EntityCodeConfig for {EntityType} on tenant {TenantId}", entityType, tenantId);

        return config;
    }

    /// <summary>
    /// Atomically increments LastSequence by <paramref name="count"/> and returns the new value.
    /// Uses a raw SQL UPDATE … RETURNING so the increment is a single DB round-trip.
    /// </summary>
    private async Task<long> IncrementSequenceAsync(
        string tenantId, string entityType, int count, CancellationToken ct)
    {
        // PostgreSQL UPDATE … RETURNING gives us the final value atomically.
        // Values are bound as parameters ({0}..{2} → DbParameters) — never interpolated
        // into the SQL text — so tenantId/entityType can never alter the statement.
        const string sql = """
            UPDATE "EntityCodeConfigs"
            SET    "LastSequence" = "LastSequence" + {0}
            WHERE  "TenantId"    = {1}
              AND  "EntityType"  = {2}
              AND  "DataStatus"  = 'Active'
            RETURNING "LastSequence"
            """;

        var results = await _context.Database
            .SqlQueryRaw<long>(sql, count, tenantId, entityType)
            .ToListAsync(ct);

        if (results.Count == 0)
            throw new InvalidOperationException(
                $"EntityCodeConfig row not found for ({tenantId}, {entityType}). " +
                "Ensure the config exists before calling GenerateAsync.");

        return results[0];
    }

    private async Task CheckAndApplyResetAsync(EntityCodeConfig config, CancellationToken ct)
    {
        if (config.ResetPeriod == CodeResetPeriod.Never) return;

        var currentKey = config.CurrentPeriodKey();
        if (config.LastResetPeriodKey == currentKey) return;

        config.MarkSequenceReset(currentKey);
        await _context.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey(config.TenantId, config.EntityType));

        _logger.LogInformation(
            "Sequence reset for {EntityType} on tenant {TenantId} — new period {Key}",
            config.EntityType, config.TenantId, currentKey);
    }

    private static long? ExtractNumericSuffix(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        // Take the last contiguous run of digits from the code
        var digits = new string(code.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
        return long.TryParse(digits, out var n) ? n : null;
    }
}
