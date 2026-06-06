namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Permanently-cached snapshot of a tenant's branding/contact information.
/// Written to Redis by IAM on every tenant create/update so that other
/// microservices (Payroll, CoreHR, etc.) can embed the data in PDFs, emails,
/// and reports without calling IAM at runtime.
/// </summary>
public sealed record TenantBrandingRecord(
    string TenantId,
    string CompanyName,
    string? CompanyEmail,
    string? LogoUrl,
    string? Country,
    string? Phone
);
