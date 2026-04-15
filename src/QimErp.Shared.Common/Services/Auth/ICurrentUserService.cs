using System.Security.Claims;

namespace QimErp.Shared.Common.Services.Auth;

public interface ICurrentUserService
{
    /// <summary>
    /// Application correlation id (typically from <c>X-Correlation-Id</c> middleware). Empty when unavailable (e.g. design-time, some consumers).
    /// </summary>
    string GetCorrelationId();

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
}
