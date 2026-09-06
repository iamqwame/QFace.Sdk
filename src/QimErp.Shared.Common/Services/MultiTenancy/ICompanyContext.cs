using QFace.Sdk.Temporal.Interceptors;

namespace QimErp.Shared.Common.Services.MultiTenancy;

public sealed record CompanyScope
{
    public required string[] AllowedCompanyIds { get; init; }
    public string? ActiveCompanyId { get; init; }
    public bool FilterActive { get; init; }
    public bool MultiCompanyEnabled { get; init; }

    public static readonly CompanyScope Inactive = new()
    {
        AllowedCompanyIds = [string.Empty],
        ActiveCompanyId = null,
        FilterActive = false,
        MultiCompanyEnabled = false
    };

    public IEnumerable<string> RealCompanyIds => AllowedCompanyIds.Where(id => !string.IsNullOrEmpty(id));

    public static CompanyScope ForCompanies(IEnumerable<string> ids, string? active)
    {
        var allowed = ids
            .Select(CompanyIdNormalizer.Normalize)
            .Append(string.Empty)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        // ActiveCompanyId is the write target and originates from the X-Company-Id header.
        // An id outside the allowed set must never become one.
        var normalizedActive = CompanyIdNormalizer.NormalizeOrNull(active);
        if (normalizedActive is not null && !allowed.Contains(normalizedActive, StringComparer.Ordinal))
        {
            normalizedActive = null;
        }

        return new CompanyScope
        {
            AllowedCompanyIds = allowed,
            ActiveCompanyId = normalizedActive,
            FilterActive = true,
            MultiCompanyEnabled = true
        };
    }

    public static CompanyScope AllCompanies(string? active)
    {
        return new CompanyScope
        {
            AllowedCompanyIds = [string.Empty],
            ActiveCompanyId = CompanyIdNormalizer.NormalizeOrNull(active),
            FilterActive = false,
            MultiCompanyEnabled = true
        };
    }
}

public interface ICompanyContext
{
    CompanyScope Current { get; }
    string[] AllowedCompanyIds { get; }
    string? ActiveCompanyId { get; }
    bool CompanyFilterActive { get; }
    bool MultiCompanyEnabled { get; }
    void SetScope(CompanyScope scope);
    void Clear();
}

/// <summary>
/// Implements both <see cref="ICompanyContext"/> (read scope + write target) and
/// <see cref="ICompanyScopeSetter"/> (used by <see cref="TenantContextActivityInterceptor"/>
/// to seed the ambient company scope for Temporal activity executions).
/// </summary>
public class CompanyContext : ICompanyContext, ICompanyScopeSetter
{
    // static AsyncLocal is what makes a singleton registration correct — the type has no instance state.
    private static readonly AsyncLocal<CompanyScope?> _scope = new();

    public static CompanyScope CurrentScope => _scope.Value ?? CompanyScope.Inactive;

    public CompanyScope Current => CurrentScope;
    public string[] AllowedCompanyIds => Current.AllowedCompanyIds;
    public string? ActiveCompanyId => Current.ActiveCompanyId;
    public bool CompanyFilterActive => Current.FilterActive;
    public bool MultiCompanyEnabled => Current.MultiCompanyEnabled;

    public void SetScope(CompanyScope scope)
    {
        _scope.Value = scope;
    }

    public void Clear()
    {
        _scope.Value = null;
    }

    // ── ICompanyScopeSetter ───────────────────────────────────────────────────
    void ICompanyScopeSetter.SetCompanyScope(string? activeCompanyId, IReadOnlyCollection<string>? allowedCompanyIds, bool filterActive)
    {
        if (filterActive)
        {
            SetScope(CompanyScope.ForCompanies(allowedCompanyIds ?? [], activeCompanyId));
            return;
        }

        // Nothing supplied must stay Inactive, not AllCompanies: AllCompanies turns multi-company
        // on, and the stamp then throws instead of stamping "" for callers that carry no company.
        if (allowedCompanyIds is null && string.IsNullOrWhiteSpace(activeCompanyId))
        {
            SetScope(CompanyScope.Inactive);
            return;
        }

        SetScope(CompanyScope.AllCompanies(activeCompanyId));
    }

    void ICompanyScopeSetter.ClearCompanyScope() => Clear();
}

public class DesignTimeCompanyContext : ICompanyContext
{
    public CompanyScope Current => CompanyScope.Inactive;
    public string[] AllowedCompanyIds => Current.AllowedCompanyIds;
    public string? ActiveCompanyId => Current.ActiveCompanyId;
    public bool CompanyFilterActive => Current.FilterActive;
    public bool MultiCompanyEnabled => Current.MultiCompanyEnabled;

    public void SetScope(CompanyScope scope) { }
    public void Clear() { }
}
