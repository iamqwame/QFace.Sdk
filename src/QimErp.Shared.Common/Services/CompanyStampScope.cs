namespace QimErp.Shared.Common.Services;

/// <summary>
/// Process-local write target for the company stamp in
/// <see cref="QimErp.Shared.Common.Interceptors.AuditEntitySaveChangesInterceptor"/>.
/// Used by Temporal activities, background jobs and seeders, where no request-scoped
/// company selection exists.
/// </summary>
public static class CompanyStampScope
{
    private static readonly AsyncLocal<string?> Override = new();

    /// <summary>An entered scope of <c>""</c> is an explicit tenant-shared target, not "unset".</summary>
    public static bool TryGetOverride(out string companyId)
    {
        var current = Override.Value;
        if (current is null)
        {
            companyId = string.Empty;
            return false;
        }

        companyId = current;
        return true;
    }

    /// <summary>
    /// Enter an override scope on the current async flow. Dispose to restore
    /// the previous value (re-entrant safe).
    /// </summary>
    public static IDisposable Enter(string companyId)
    {
        var previous = Override.Value;
        Override.Value = companyId ?? string.Empty;
        return new Restorer(previous);
    }

    /// <summary>Stamp <c>""</c> — reference and lookup seeding that is genuinely tenant-wide.</summary>
    public static IDisposable EnterShared() => Enter(string.Empty);

    private sealed class Restorer(string? previous) : IDisposable
    {
        public void Dispose() => Override.Value = previous;
    }
}
