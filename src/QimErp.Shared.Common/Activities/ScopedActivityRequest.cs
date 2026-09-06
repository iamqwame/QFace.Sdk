namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Base shape for Temporal activity requests. TenantContextActivityInterceptor reads these
/// top-level properties to seed tenant identity and company scope before the activity body runs.
/// </summary>
public record ScopedActivityRequest(
    string TenantId,
    string CompanyId,
    string UserId,
    string UserEmail,
    string UserName);
