using System.Security.Claims;

namespace QimErp.Shared.Common.Services;

public class DesignTimeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated => false;
    public string GetUserId() => "design-time-user";
    public string? GetRole() => null;
    public List<string> GetUserRoles() => [];
    public string GetTenantId() => string.Empty;
    public string? GetToken() => null;
    public IEnumerable<Claim> GetClaims() => [];
    public string GetUserEmail() => "design-time@migrations.local";
    public string GetUserName() => "Design Time User";
    public string? GetDomainName() => null;
    public string? GetLanguage() => null;
    public string? GetTimeZone() => null;
    public string? GetCompanyName() => null;
    public string? GetEmployeeId() => null;
    public string? GetRankId() => null;
    public string? GetRankName() => null;
    public string? GetOrganizationalUnitId() => null;
    public string? GetOrganizationalUnitName() => null;
    public List<string> GetRoleIds() => [];
}