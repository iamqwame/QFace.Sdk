namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Per-tenant, per-entity configuration for human-readable code generation.
/// Lives in QFace.Sdk so every module inherits it via ApplicationDbContext.
///
/// Generated code shape: {Prefix}{Separator}{Year?}{Separator?}{SEQ:PaddingWidth}
/// Example: Prefix="OT", Separator="-", IncludeYear=true, PaddingWidth=4
///          → OT-2026-0001
/// </summary>
public sealed class EntityCodeConfig : GuidAuditableEntity
{
    // ── Identity ────────────────────────────────────────────────────────────

    /// <summary>Logical entity name used as the lookup key. E.g. "Employee", "OvertimeRecord".</summary>
    public string EntityType { get; private set; } = string.Empty;

    // ── Format ──────────────────────────────────────────────────────────────

    /// <summary>Code prefix. E.g. "OT", "EMP", "LN". Empty string is valid.</summary>
    public string Prefix { get; private set; } = string.Empty;

    /// <summary>Separator between segments. E.g. "-", "/", "". Defaults to "-".</summary>
    public string Separator { get; private set; } = "-";

    /// <summary>When true, the 4-digit year is inserted between prefix and sequence.</summary>
    public bool IncludeYear { get; private set; } = true;

    /// <summary>Minimum digits in the sequence number, zero-padded. Defaults to 4.</summary>
    public int PaddingWidth { get; private set; } = 4;

    // ── Sequence state ──────────────────────────────────────────────────────

    /// <summary>
    /// The last sequence value that was issued.
    /// Incremented atomically via a single UPDATE … RETURNING statement.
    /// </summary>
    public long LastSequence { get; private set; } = 0;

    /// <summary>
    /// Stores the highest numeric value seen in manually-entered codes.
    /// Used during the manual → auto transition to continue from the right number.
    /// </summary>
    public long ManualHighWaterMark { get; private set; } = 0;

    // ── Mode ────────────────────────────────────────────────────────────────

    public CodeGenerationMode Mode { get; private set; } = CodeGenerationMode.Auto;

    /// <summary>When/if the sequence resets to 1.</summary>
    public CodeResetPeriod ResetPeriod { get; private set; } = CodeResetPeriod.Never;

    /// <summary>The year (and month when applicable) the sequence was last reset.</summary>
    public string? LastResetPeriodKey { get; private set; }

    // ── EF constructor ──────────────────────────────────────────────────────
    private EntityCodeConfig() { }

    // ── Factory ─────────────────────────────────────────────────────────────

    public static EntityCodeConfig Create(
        string tenantId,
        string entityType,
        string prefix = "",
        string separator = "-",
        bool includeYear = true,
        int paddingWidth = 4,
        CodeGenerationMode mode = CodeGenerationMode.Auto,
        CodeResetPeriod resetPeriod = CodeResetPeriod.Never)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(entityType));
        if (paddingWidth < 1 || paddingWidth > 20)
            throw new ArgumentOutOfRangeException(nameof(paddingWidth), "PaddingWidth must be between 1 and 20.");

        var config = new EntityCodeConfig
        {
            Id = CreateId(),
            TenantId = tenantId,
            EntityType = entityType.Trim(),
            Prefix = prefix.Trim(),
            Separator = separator,
            IncludeYear = includeYear,
            PaddingWidth = paddingWidth,
            Mode = mode,
            ResetPeriod = resetPeriod,
            LastSequence = 0,
            ManualHighWaterMark = 0,
        };
        config.AsActive();
        return config;
    }

    // ── Mutation ─────────────────────────────────────────────────────────────

    public void UpdateFormat(string prefix, string separator, bool includeYear, int paddingWidth)
    {
        if (paddingWidth < 1 || paddingWidth > 20)
            throw new ArgumentOutOfRangeException(nameof(paddingWidth));
        Prefix = prefix.Trim();
        Separator = separator;
        IncludeYear = includeYear;
        PaddingWidth = paddingWidth;
    }

    public void SetMode(CodeGenerationMode mode) => Mode = mode;

    public void SetResetPeriod(CodeResetPeriod period) => ResetPeriod = period;

    /// <summary>Called during manual→auto reconciliation to set the starting point.</summary>
    public void SetManualHighWaterMark(long value)
    {
        if (value > ManualHighWaterMark)
            ManualHighWaterMark = value;
        // LastSequence starts at the high water mark so next auto code is +1
        if (value > LastSequence)
            LastSequence = value;
    }

    /// <summary>Records that the sequence was reset in this period.</summary>
    public void MarkSequenceReset(string periodKey)
    {
        LastSequence = 0;
        LastResetPeriodKey = periodKey;
    }

    // ── Code formatting ──────────────────────────────────────────────────────

    /// <summary>
    /// Formats a raw sequence value into the final human-readable code.
    /// Does NOT advance the sequence — call this after reserving the value.
    /// </summary>
    public string FormatCode(long sequenceValue, DateTimeOffset? at = null)
    {
        var seq = sequenceValue.ToString().PadLeft(PaddingWidth, '0');
        var date = at ?? DateTimeOffset.UtcNow;

        if (!string.IsNullOrEmpty(Prefix) && IncludeYear)
            return $"{Prefix}{Separator}{date.Year}{Separator}{seq}";

        if (!string.IsNullOrEmpty(Prefix))
            return $"{Prefix}{Separator}{seq}";

        if (IncludeYear)
            return $"{date.Year}{Separator}{seq}";

        return seq;
    }

    /// <summary>
    /// Returns the current period key for reset detection.
    /// E.g. "2026" for yearly, "2026-05" for monthly.
    /// </summary>
    public string CurrentPeriodKey(DateTimeOffset? at = null)
    {
        var date = at ?? DateTimeOffset.UtcNow;
        return ResetPeriod switch
        {
            CodeResetPeriod.Yearly  => date.Year.ToString(),
            CodeResetPeriod.Monthly => $"{date.Year}-{date.Month:D2}",
            _                       => "never",
        };
    }
}

// ── Enums ────────────────────────────────────────────────────────────────────

public enum CodeGenerationMode
{
    /// <summary>System generates codes automatically on record create.</summary>
    Auto,
    /// <summary>User enters the code manually; system validates uniqueness.</summary>
    Manual,
}

public enum CodeResetPeriod
{
    /// <summary>Sequence never resets (default, safest).</summary>
    Never,
    /// <summary>Resets to 1 on the first use in a new calendar year.</summary>
    Yearly,
    /// <summary>Resets to 1 on the first use in a new calendar month.</summary>
    Monthly,
}
