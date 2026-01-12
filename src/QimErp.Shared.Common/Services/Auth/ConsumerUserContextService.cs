using System.Security.Claims;

namespace QimErp.Shared.Common.Services.Auth;

/// <summary>
/// Consumer-specific implementation of ICurrentUserService that uses AsyncLocal for context storage
/// Used in conjunction with ConsumerAuditExtensions.WithAuditContextAsync
/// </summary>
public class ConsumerUserContextService : ICurrentUserService
{
    private static readonly AsyncLocal<ConsumerContext> Context = new();

    public void SetContext(string tenantId, string userEmail, string? userName = null, string? triggeredBy = null)
    {
        Context.Value = new ConsumerContext
        {
            TenantId = tenantId,
            TriggeredBy = triggeredBy ?? "system",
            UserEmail = userEmail,
            UserName = userName ?? "system",
            Timestamp = DateTime.UtcNow
        };
    }

    public void ClearContext()
    {
        Context.Value = null;
    }

    public bool IsAuthenticated
    {
        get
        {
            var context = Context.Value;
            if (context == null) return false;
            var userId = context.TriggeredBy;
            return !string.IsNullOrEmpty(userId) && userId != "system" && userId != "anonymous";
        }
    }

    public string GetUserId()
    {
        return Context.Value?.TriggeredBy ?? "system";
    }

    public string? GetRole()
    {
        return "Consumer";
    }

    public List<string> GetUserRoles()
    {
        return ["Consumer"];
    }

    public string GetTenantId()
    {
        return Context.Value?.TenantId ?? string.Empty;
    }

    public string? GetToken()
    {
        return null;
    }

    public IEnumerable<Claim> GetClaims()
    {
        var context = Context.Value;
        if (context == null)
            return [];

        return
        [
            new Claim("tenantId", context.TenantId),
            new Claim("userId", context.TriggeredBy),
            new Claim("userEmail", context.UserEmail),
            new Claim("userName", context.UserName),
            new Claim("role", "Consumer")
        ];
    }

    public string GetUserEmail()
    {
        return Context.Value?.UserEmail ?? "system@consumer";
    }

    public string GetUserName()
    {
        return Context.Value?.UserName ?? "system";
    }

    public string? GetDomainName()
    {
        return null;
    }

    public string? GetLanguage()
    {
        return null;
    }

    public string? GetTimeZone()
    {
        return null;
    }

    public string? GetCompanyName()
    {
        return null;
    }

    public string? GetEmployeeId() => null;
    public string? GetRankId() => null;
    public string? GetRankName() => null;
    public string? GetOrganizationalUnitId() => null;
    public string? GetOrganizationalUnitName() => null;
    public List<string> GetRoleIds() => [];

    private class ConsumerContext
    {
        public string TenantId { get; set; } = string.Empty;
        public string TriggeredBy { get; set; } = "system";
        public string UserEmail { get; set; } = "system@consumer";
        public string UserName { get; set; } = "system";
        public DateTime Timestamp { get; set; }
    }
}
