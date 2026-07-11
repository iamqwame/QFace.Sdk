namespace QimErp.Shared.Common.Activities.TenantOnboarding;

/// <summary>
/// Shared request type for starting the TenantOnboardingWorkflow on Platform Orchestration.
/// Used both by the IAM module (to dispatch the workflow via Temporal) and by the
/// Platform Orchestration workflow internally.
/// </summary>
public class TenantOnboardingStartRequest
{
    public Guid TenantId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string SuperAdminEmail { get; set; } = string.Empty;
    public string SuperAdminFirstName { get; set; } = string.Empty;
    public string SuperAdminLastName { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public string[] SelectedModules { get; set; } = [];
    public string? SelectedBundle { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public string? CallbackUrl { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? CountryCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? IndustryType { get; set; }
    /// <summary>Work / corporate email — used for the employee record.</summary>
    public string WorkEmail { get; set; } = string.Empty;
    /// <summary>Personal email — additional notification recipient if provided.</summary>
    public string? PersonalEmail { get; set; }
    public string EmployeeCount { get; set; } = string.Empty;
    public decimal? TotalCost { get; set; }
    /// <summary>Display name of the selected plan (e.g. "Starter", "Pro").</summary>
    public string Plan { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
}
