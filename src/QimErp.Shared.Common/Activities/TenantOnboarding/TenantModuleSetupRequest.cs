namespace QimErp.Shared.Common.Activities.TenantOnboarding;

public class TenantModuleSetupRequest
{
    public string Action { get; set; } = ""; // EnsureSubscription | SetupTenant | CreateFirstEmployee | SyncEmployee
    public string TenantId { get; set; } = "";
    public string? Company { get; set; }
    public string? Domain { get; set; }
    public string? IndustryType { get; set; }
    public string? CompanyType { get; set; }
    public string? PlanType { get; set; }
    public string? BillingCycle { get; set; }
    public List<string>? SelectedModules { get; set; }
    public string? SelectedBundle { get; set; }
    public int? UserCount { get; set; }
    public decimal? TotalCost { get; set; }
    public string? CallbackUrl { get; set; }
    // Employee fields (CreateFirstEmployee + SyncEmployee)
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeFirstName { get; set; }
    public string? EmployeeLastName { get; set; }
    public string? EmployeeEmail { get; set; }
    public string? SuperAdminEmail { get; set; }
    public string? SuperAdminFirstName { get; set; }
    public string? SuperAdminLastName { get; set; }
    public string? JobTitle { get; set; }
    public string? MobileNumber { get; set; }
    public string? CountryCode { get; set; }
    public string? Country { get; set; }
    public string? PersonalEmail { get; set; }
    public string? WorkEmail { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Plan { get; set; }
    public string TriggeredBy { get; set; } = "";
}
