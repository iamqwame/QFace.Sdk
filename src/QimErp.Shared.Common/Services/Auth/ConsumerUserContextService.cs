using System.Security.Claims;
using Microsoft.Extensions.Options;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Options;

namespace QimErp.Shared.Common.Services.Auth;

/// <summary>
/// Consumer-specific implementation of ICurrentUserService that uses AsyncLocal for context storage
/// Used in conjunction with ConsumerAuditExtensions.WithAuditContextAsync
/// </summary>
public class ConsumerUserContextService : ICurrentUserService, ITenantContextSetter
{
    private static readonly AsyncLocal<ConsumerContext?> Context = new();
    private readonly SystemOptions _systemOptions;

    public ConsumerUserContextService(IOptions<SystemOptions> systemOptions)
    {
        _systemOptions = systemOptions.Value;
    }

    // Explicit interface implementation to satisfy ICurrentUserService.SetContext(4-param).
    void ICurrentUserService.SetContext(string tenantId, string userEmail, string? userName, string? userId)
        => SetContext(tenantId, userEmail, userName, userId);

    public void SetContext(string tenantId, string userEmail, string? userName = null, string? triggeredBy = null, string? correlationId = null)
    {
        Context.Value = new ConsumerContext
        {
            TenantId = tenantId,
            TriggeredBy = triggeredBy ?? _systemOptions.DefaultUserId,
            UserEmail = userEmail,
            UserName = userName ?? _systemOptions.DefaultUserId,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }

    public void ClearContext()
    {
        Context.Value = null;
    }

    public string GetCorrelationId() => Context.Value?.CorrelationId ?? string.Empty;

    public bool IsAuthenticated
    {
        get
        {
            var context = Context.Value;
            if (context == null) return false;
            var userId = context.TriggeredBy;
            return !string.IsNullOrEmpty(userId) && userId != _systemOptions.DefaultUserId && userId != "anonymous";
        }
    }

    public string GetUserId()
    {
        return Context.Value?.TriggeredBy ?? _systemOptions.DefaultUserId;
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
        return Context.Value?.UserEmail ?? _systemOptions.ConsumerSystemEmail;
    }

    public string GetUserName()
    {
        return Context.Value?.UserName ?? _systemOptions.DefaultUserId;
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

    public IReadOnlyList<string> GetPermissions() => [];

    // ── ITenantContextSetter ──────────────────────────────────────────────────
    void ITenantContextSetter.SetTenantContext(string tenantId, string userEmail, string? userName, string? userId)
        => SetContext(tenantId, userEmail, userName, userId);

    void ITenantContextSetter.ClearTenantContext() => ClearContext();

    private class ConsumerContext
    {
        public string TenantId { get; set; } = string.Empty;
        public string TriggeredBy { get; set; } = "system";
        public string UserEmail { get; set; } = "system@consumer";
        public string UserName { get; set; } = "system";
        public string? CorrelationId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
