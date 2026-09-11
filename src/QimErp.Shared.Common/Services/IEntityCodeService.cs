namespace QimErp.Shared.Common.Services;

/// <summary>
/// Central service for generating and managing human-readable codes across all modules.
/// Every service uses this via QFace.Sdk — no module implements its own sequence logic.
/// </summary>
public interface IEntityCodeService
{
    /// <summary>
    /// Atomically reserves and returns the next formatted code for the given entity type.
    /// Concurrent callers are serialised by the DB — no two calls return the same value.
    /// Auto-creates a default config if none exists for this tenant + entity type.
    /// </summary>
    Task<string> GenerateAsync(string tenantId, string entityType, CancellationToken ct = default);

    /// <summary>
    /// Atomically reserves <paramref name="count"/> codes in a single DB round-trip.
    /// Designed for bulk import scenarios — vastly faster than calling GenerateAsync N times.
    /// Returns codes in ascending sequence order.
    /// </summary>
    Task<string[]> GenerateBatchAsync(string tenantId, string entityType, int count, CancellationToken ct = default);

    /// <summary>
    /// Returns the next likely code WITHOUT reserving it (advisory / preview only).
    /// The returned code MAY already be taken by the time the caller submits.
    /// Callers MUST display a "not reserved" indicator alongside this value.
    /// Skips codes held by an active reservation from another token.
    /// </summary>
    Task<string> SuggestAsync(string tenantId, string entityType, CancellationToken ct = default);

    /// <summary>
    /// Returns the current config for this tenant + entity type, or null if not configured.
    /// </summary>
    Task<EntityCodeConfig?> GetConfigAsync(string tenantId, string entityType, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates the code generation config for a tenant + entity type.
    /// </summary>
    Task UpsertConfigAsync(string tenantId, string entityType,
        string prefix, string separator, bool includeYear, int paddingWidth,
        CodeGenerationMode mode, CodeResetPeriod resetPeriod,
        CancellationToken ct = default);

    /// <summary>
    /// Scans existing codes for this entity type, extracts the highest numeric value,
    /// and advances the sequence so the next auto-generated code is higher than any manual one.
    /// Should be called when switching from Manual → Auto mode.
    /// </summary>
    Task<long> ReconcileManualToAutoAsync(string tenantId, string entityType,
        IEnumerable<string> existingCodes, CancellationToken ct = default);

    /// <summary>
    /// Entity type keys this module actually generates codes for. Drives admin/settings UIs
    /// that list every numbering rule a module exposes — never includes another module's types.
    /// </summary>
    IReadOnlyCollection<string> GetKnownEntityTypes();

    /// <summary>
    /// Returns the current (or auto-created) config for every entity type this service knows about.
    /// </summary>
    Task<IReadOnlyList<EntityCodeConfig>> GetAllConfigsAsync(string tenantId, CancellationToken ct = default);

    /// <summary>
    /// The module that owns numbering for this entity type (e.g. "Payroll", "Inventory") — always
    /// this service's own module, since every entity type it knows about is one it registered itself.
    /// </summary>
    string GetModuleFor(string entityType);

    /// <summary>
    /// Validates that <paramref name="code"/> round-trips through the entity type's format rules.
    /// </summary>
    Task<(bool IsValid, string? Error)> ValidateManualAsync(
        string tenantId, string entityType, string code, CancellationToken ct = default);

    /// <summary>
    /// Holds <paramref name="code"/> for <paramref name="ttl"/> under <paramref name="reservationToken"/>.
    /// When <paramref name="validateFormat"/> is true (default), runs <see cref="ValidateManualAsync"/> first.
    /// Callers that validate elsewhere (e.g. ChartOfAccount ranges) pass <c>validateFormat: false</c>.
    /// </summary>
    Task<(bool Reserved, string? Error)> TryReserveManualAsync(
        string tenantId, string entityType, string code, string reservationToken, TimeSpan ttl,
        bool validateFormat = true, string? metadata = null, CancellationToken ct = default);

    /// <summary>Releases a hold when the create dialog is abandoned or the code changes.</summary>
    Task ReleaseReservationAsync(
        string tenantId, string entityType, string code, string reservationToken,
        CancellationToken ct = default);

    /// <summary>Removes the hold after a successful create so the code can never be re-suggested as free.</summary>
    Task ConsumeReservationAsync(
        string tenantId, string entityType, string code, string reservationToken,
        CancellationToken ct = default);

    /// <summary>
    /// True when an active reservation for this code exists under a different token.
    /// Used by suggest/generate skip logic and ChartOfAccount next-code allocation.
    /// </summary>
    Task<bool> IsCodeReservedByOtherAsync(
        string tenantId, string entityType, string code, string? myToken = null,
        CancellationToken ct = default);
}
