namespace QimErp.Shared.Common.TenantSetup;

public static class ModuleKeys
{
    public const string CoreHR               = "CoreHR";
    public const string Payroll              = "Payroll";
    public const string Leave                = "Leave";
    public const string Recruitment          = "Recruitment";
    public const string Benefits             = "Benefits";
    public const string Surveys              = "Surveys";
    public const string EmployeeEngagement   = "EmployeeEngagement";
    public const string Learning             = "Learning";
    public const string Performance          = "Performance";
    public const string Talent               = "Talent";
    public const string WorkforcePlanning    = "WorkforcePlanning";
    public const string Workflow             = "Workflow";

    // Added for the App Store catalog — these modules previously had no ModuleKeys constant
    // at all. Accounting is 5 separate sub-ledgers in TenantBilling's PricingModule catalog
    // (COREACCOUNTING/ACCOUNTSPAYABLE/ACCOUNTSRECEIVABLE/BUDGETPLANNING/CASHMANAGEMENT) and 3
    // separate TenantOnboardingWorkflow steps (SetupAccountingGlTenant/ApTenant/ArTenant) — kept
    // as 5 separate constants here to match, not lumped into one generic "Accounting".
    public const string CoreAccounting       = "CoreAccounting";
    public const string AccountsPayable      = "AccountsPayable";
    public const string AccountsReceivable   = "AccountsReceivable";
    public const string BudgetPlanning       = "BudgetPlanning";
    public const string CashManagement       = "CashManagement";
    public const string Inventory            = "Inventory";
    public const string Project              = "Project";
}
