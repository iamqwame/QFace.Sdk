using QimErp.Shared.Common.Services.MultiTenancy;

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

    /// <summary>
    /// Tenant-shared stamp for a caller whose scope really does cover the whole tenant.
    /// A caller holding a company list but no active selection is refused: the row would otherwise
    /// be served to every company in the tenant, including ones outside that caller's claim.
    /// </summary>
    public static IDisposable EnterSharedAsTenantWideWriter(string subject)
    {
        var scope = CompanyContext.CurrentScope;
        if (scope.IsTenantWideWriter)
            return EnterShared();

        throw new AppSettingScopeViolationException(
            $"{subject} has no company write target: {scope.RealCompanyIds.Count()} company/companies are in scope " +
            "and none was selected as active. Send the X-Company-Id header to choose one.");
    }

    private sealed class Restorer(string? previous) : IDisposable
    {
        public void Dispose() => Override.Value = previous;
    }
}
