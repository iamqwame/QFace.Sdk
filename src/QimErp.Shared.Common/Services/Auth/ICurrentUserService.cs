using System.Security.Claims;

namespace QimErp.Shared.Common.Services.Auth;

public interface ICurrentUserService
{
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
