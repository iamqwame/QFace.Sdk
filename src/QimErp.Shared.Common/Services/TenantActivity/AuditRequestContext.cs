using Microsoft.AspNetCore.Http;
using QFace.Sdk.Extensions;

namespace QimErp.Shared.Common.Services.TenantActivity;

/// <summary>
/// Request-scoped audit metadata captured from the current HTTP context.
/// </summary>
public sealed record AuditRequestContext(string? IpAddress, string? UserAgent, string? SessionId)
{
    public static AuditRequestContext? TryCapture(IHttpContextAccessor? httpContextAccessor)
    {
        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        var ipAddress = httpContext.GetClientIpAddress();
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "0.0.0.0")
        {
            ipAddress = null;
        }

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = null;
        }

        string? sessionId = null;
        if (httpContext.Items.TryGetValue(SessionContextKeys.CurrentSessionId, out var sessionValue)
            && sessionValue is string sessionIdValue
            && !string.IsNullOrWhiteSpace(sessionIdValue))
        {
            sessionId = sessionIdValue;
        }

        if (ipAddress is null && userAgent is null && sessionId is null)
        {
            return null;
        }

        return new AuditRequestContext(ipAddress, userAgent, sessionId);
    }
}
