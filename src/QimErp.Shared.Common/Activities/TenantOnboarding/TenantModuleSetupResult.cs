namespace QimErp.Shared.Common.Activities.TenantOnboarding;

public class TenantModuleSetupResult
{
    public bool Success { get; set; }
    public bool AlreadyExists { get; set; }
    public string? ErrorMessage { get; set; }
    // EnsureSubscription result
    public Guid? SubscriptionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionReference { get; set; }
    // CreateFirstEmployee result
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeNo { get; set; }
}
