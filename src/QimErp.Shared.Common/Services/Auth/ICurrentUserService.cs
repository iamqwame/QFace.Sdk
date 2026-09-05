using System.Security.Claims;

namespace QimErp.Shared.Common.Services.Auth;

public interface ICurrentUserService
{
    /// <summary>
    /// Application correlation id (typically from <c>X-Correlation-Id</c> middleware). Empty when unavailable (e.g. design-time, some consumers).
    /// </summary>
    string GetCorrelationId();

    /// <summary>
    /// Seeds ambient identity for non-HTTP execution contexts (Temporal activities, background jobs,
    /// message consumers). Stored in <c>AsyncLocal</c> — scoped to the current async call chain.
    /// Call once at the entry point of a background job; the EF interceptor picks it up automatically.
    /// </summary>
    void SetContext(string tenantId, string userEmail, string? userName = null, string? userId = null);

    /// <summary>Clears the ambient context set by <see cref="SetContext"/>. Call in a finally block.</summary>
    void ClearContext();

    bool IsAuthenticated { get; }
    string GetUserId();
    string? GetRole();
    List<string> GetUserRoles();
    string GetTenantId();
    string? GetToken();
    IEnumerable<Claim> GetClaims();
    string GetUserEmail();
    string GetUserName();
    string? GetDomainName();
    string? GetLanguage();
    string? GetTimeZone();
    string? GetCompanyName();
    string? GetEmployeeId();
    string? GetRankId();
    string? GetRankName();
    string? GetOrganizationalUnitId();
    string? GetOrganizationalUnitName();
    List<string> GetRoleIds();

    /// <summary>Permission codes carried by the caller's token. Empty — never null — when no permission claim is present.</summary>
    IReadOnlyList<string> GetPermissions();
}
