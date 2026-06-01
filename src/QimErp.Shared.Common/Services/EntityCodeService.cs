using QimErp.Shared.Common.Entities;

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
/// </summary>
public abstract class EntityCodeService<TContext> : IEntityCodeService
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly ILogger _logger;

    // Default configs applied when no config row exists for a given entity type
    private static readonly Dictionary<string, (string Prefix, string Separator, bool IncludeYear, int PaddingWidth)>
        _defaults = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Employee"]        = ("EMP", "",  false, 7),
            ["OvertimeRecord"]  = ("OT",  "-", true,  4),
            ["Loan"]            = ("LN",  "-", true,  4),
            ["SalaryAdvance"]   = ("ADV", "-", true,  4),
            ["PayrollRun"]      = ("PR",  "-", true,  4),
            ["Allowance"]       = ("ALL", "-", false, 4),
            ["Deduction"]       = ("DED", "-", false, 4),
            ["Vendor"]          = ("VEN", "",  false, 6),
            ["Invoice"]         = ("INV", "-", true,  5),
        };

    protected EntityCodeService(TContext context, ILogger logger)
    {
        _context = context;
        _logger  = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

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
        return await _context.Set<EntityCodeConfig>()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.EntityType == entityType, ct);
    }

    public async Task UpsertConfigAsync(
        string tenantId, string entityType,
        string prefix, string separator, bool includeYear, int paddingWidth,
        CodeGenerationMode mode, CodeResetPeriod resetPeriod,
        CancellationToken ct = default)
    {
        var existing = await GetConfigAsync(tenantId, entityType, ct);
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
    }

    public async Task<long> ReconcileManualToAutoAsync(
        string tenantId, string entityType,
        IEnumerable<string> existingCodes, CancellationToken ct = default)
    {
        // Extract all numeric values from existing codes, take the max.
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

        _logger.LogInformation(
            "Reconciled manual→auto for {EntityType} on tenant {TenantId}: high-water={HWM}",
            entityType, tenantId, maxFound);

        return maxFound;
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private async Task<EntityCodeConfig> GetOrCreateConfigAsync(
        string tenantId, string entityType, CancellationToken ct)
    {
        var config = await GetConfigAsync(tenantId, entityType, ct);
        if (config is not null) return config;

        // Auto-create from defaults (or bare minimum if entity type is unknown)
        _defaults.TryGetValue(entityType, out var def);
        config = EntityCodeConfig.Create(
            tenantId, entityType,
            prefix:      def.Prefix      ?? entityType[..Math.Min(3, entityType.Length)].ToUpper(),
            separator:   def.Separator   ?? "-",
            includeYear: def.IncludeYear,
            paddingWidth: def.PaddingWidth > 0 ? def.PaddingWidth : 4);

        _context.Set<EntityCodeConfig>().Add(config);
        await _context.SaveChangesAsync(ct);

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
        var sql = $"""
            UPDATE "EntityCodeConfigs"
            SET    "LastSequence" = "LastSequence" + {count}
            WHERE  "TenantId"    = '{tenantId}'
              AND  "EntityType"  = '{entityType}'
              AND  "DataStatus"  = 'Active'
            RETURNING "LastSequence"
            """;

        var results = await _context.Database
            .SqlQueryRaw<long>(sql)
            .ToListAsync(ct);

        if (results.Count == 0)
            throw new InvalidOperationException(
                $"EntityCodeConfig row not found for ({tenantId}, {entityType}). " +
                "Ensure the config exists before calling GenerateAsync.");

        return results[0];
    }

    /// <summary>
    /// Checks whether the sequence should reset for a new period (year / month).
    /// Only runs when ResetPeriod ≠ Never.
    /// </summary>
    private async Task CheckAndApplyResetAsync(EntityCodeConfig config, CancellationToken ct)
    {
        if (config.ResetPeriod == CodeResetPeriod.Never) return;

        var currentKey = config.CurrentPeriodKey();
        if (config.LastResetPeriodKey == currentKey) return;

        config.MarkSequenceReset(currentKey);
        await _context.SaveChangesAsync(ct);

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
