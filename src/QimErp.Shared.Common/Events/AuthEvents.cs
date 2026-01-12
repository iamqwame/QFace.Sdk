namespace QimErp.Shared.Common.Events;

public class TwoFactorEnabledEvent : DomainEvent
{
    public string UserId { get; init; }
    public string Method { get; init; } // "AuthenticatorApp", "SMS", or "Email"

    public TwoFactorEnabledEvent(
        string tenantId,
        string userEmail,
        string userId,
        string method,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        UserId = userId;
        Method = method;
    }
}

public class TwoFactorDisabledEvent : DomainEvent
{
    public string UserId { get; init; }
    public string Method { get; init; } // "AuthenticatorApp", "SMS", or "Email"

    public TwoFactorDisabledEvent(
        string tenantId,
        string userEmail,
        string userId,
        string method,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        UserId = userId;
        Method = method;
    }
}

public class TwoFactorVerificationFailedEvent : DomainEvent
{
    public string UserId { get; init; }
    public string? FailureReason { get; init; }
    public string? IpAddress { get; init; }

    public TwoFactorVerificationFailedEvent(
        string tenantId,
        string userEmail,
        string userId,
        string? failureReason = null,
        string? ipAddress = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        UserId = userId;
        FailureReason = failureReason;
        IpAddress = ipAddress;
    }
}

