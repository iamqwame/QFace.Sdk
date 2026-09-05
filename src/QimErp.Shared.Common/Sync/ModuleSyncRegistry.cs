using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Canonical registry for install wiring and runtime sync fan-out.
/// Special cases (not gated by a single module key):
/// <list type="bullet">
/// <item><b>IAM</b> — <see cref="SyncType.Employee"/> subscriber with <c>RequiresModuleSelection=false</c>; invitation/password flags only.</item>
/// <item><b>TenantBilling</b> — employee seat count; always runs when employee sync fires.</item>
/// <item><b>Workflow</b> — actor sync when <see cref="ModuleKeys.Workflow"/> is selected; handled in <c>EmployeeSyncWorkflow</c>, not this table.</item>
/// </list>
/// Cross-module business events (AP↔Inventory, AR↔Project) stay on dedicated per-contract workflows — not install fan-out.
/// </summary>
public static class ModuleSyncRegistry
{
    public const string LoadTenantModulesQueue = "qimerp-iam-tenant-activity";
    public const string LoadTenantModulesActivityName = "LoadTenantModules";

    private static readonly ModuleSyncDefinition[] Modules =
    [
        new("core-hr", ModuleKeys.CoreHR, [], SampleSeedStep: "SeedSampleCoreHr"),
        new("payroll", ModuleKeys.Payroll,
            ["SetupPayrollTenant", "SetupPayrollTenantConfig"],
            EmployeeBackfillStep: "EnsureEmployeeInPayroll",
            AdminDataBackfillStep: "EnsureAdminDataInPayroll",
            SampleSeedStep: "SeedSamplePayroll"),
        new("leave", ModuleKeys.Leave, ["SetupLeaveTenant"],
            EmployeeBackfillStep: "EnsureEmployeeInLeave",
            AdminDataBackfillStep: "EnsureAdminDataInLeave",
            SampleSeedStep: "SeedSampleLeave"),
        new("recruitment", ModuleKeys.Recruitment,
            ["SetupRecruitmentTenant", "InitialSetupRecruitment"],
            EmployeeBackfillStep: "EnsureEmployeeInRecruitment",
            AdminDataBackfillStep: "EnsureAdminDataInRecruitment",
            SampleSeedStep: "SeedSampleRecruitment"),
        new("workflows", ModuleKeys.Workflow, ["InitialSetupWorkflow"], DisableStep: "DisableWorkflowModule", SampleSeedStep: "SeedSampleWorkflows"),
        new("performance", ModuleKeys.Performance,
            ["InitialSetupPerformance"],
            EmployeeBackfillStep: "EnsureEmployeeInPerformance",
            AdminDataBackfillStep: "EnsureAdminDataInPerformance",
            SampleSeedStep: "SeedSamplePerformance"),
        new("benefits", ModuleKeys.Benefits,
            ["SetupBenefitTenant", "InitialSetupBenefit"],
            EmployeeBackfillStep: "EnsureEmployeeInBenefit",
            AdminDataBackfillStep: "EnsureAdminDataInBenefit"),
        new("surveys", ModuleKeys.Surveys,
            ["SetupSurveysTenant", "InitialSetupSurveys"],
            EmployeeBackfillStep: "EnsureEmployeeInSurveys",
            AdminDataBackfillStep: "EnsureAdminDataInSurveys",
            SampleSeedStep: "SeedSampleSurveys"),
        new("employee-engagement", ModuleKeys.EmployeeEngagement,
            ["InitialSetupEmployeeEngagement"],
            EmployeeBackfillStep: "EnsureEmployeeInEmployeeEngagement",
            AdminDataBackfillStep: "EnsureAdminDataInEmployeeEngagement",
            SampleSeedStep: "SeedSampleEmployeeEngagement"),
        new("learning", ModuleKeys.Learning,
            ["InitialSetupLearning"],
            EmployeeBackfillStep: "EnsureEmployeeInLearning"),
        new("talent", ModuleKeys.Talent,
            ["InitialSetupTalent"],
            EmployeeBackfillStep: "EnsureEmployeeInTalent"),
        new("workforce-planning", ModuleKeys.WorkforcePlanning,
            ["InitialSetupWorkforcePlanning"],
            EmployeeBackfillStep: "EnsureEmployeeInWorkforcePlanning",
            SampleSeedStep: "SeedSampleWorkforcePlanning"),
        new("core-accounting", ModuleKeys.CoreAccounting,
            ["SetupAccountingGlTenant"],
            AdminDataBackfillStep: "EnsureAdminDataInAccounting"),
        new("accounts-payable", ModuleKeys.AccountsPayable,
            ["SetupAccountingApTenant"],
            RequiresItemKey: "core-accounting",
            PrerequisiteItemKeys: ["core-accounting"]),
        new("accounts-receivable", ModuleKeys.AccountsReceivable,
            ["SetupAccountingArTenant"],
            RequiresItemKey: "core-accounting",
            PrerequisiteItemKeys: ["core-accounting"]),
        new("budget-planning", ModuleKeys.BudgetPlanning,
            ["SetupBudgetPlanningTenant"],
            RequiresItemKey: "core-accounting",
            PrerequisiteItemKeys: ["core-accounting"]),
        new("cash-management", ModuleKeys.CashManagement,
            ["SetupCashManagementTenant"],
            RequiresItemKey: "core-accounting",
            PrerequisiteItemKeys: ["core-accounting"]),
        new("inventory", ModuleKeys.Inventory, ["SetupOperationsInventoryTenant"], SampleSeedStep: "SeedSampleInventory"),
        new("pos", ModuleKeys.POS, ["SetupPosTenant"], RequiresItemKey: "inventory", PrerequisiteItemKeys: ["inventory"], SampleSeedStep: "SeedSamplePos"),
        new("project", ModuleKeys.Project,
            ["SetupOperationsProjectTenant"],
            EmployeeBackfillStep: "EnsureEmployeeInProject",
            AdminDataBackfillStep: "EnsureAdminDataInProject"),

        // Attendance — catalog module stub so biometric-sync plugin can declare a prerequisite.
        new("attendance", ModuleKeys.Attendance, ["SetupAttendanceTenant"]),
        new("qim-ai", ModuleKeys.QimAI, []),
        new("reporting", ModuleKeys.Reporting, []),

        // Plugins — narrower than a module install: flip on one capability inside an
        // already-installed module, via the module's own idempotent Enable/Disable activity.
        // ModuleKey is the OWNING module (used only to build the outgoing SelectedModules
        // payload for the setup-step dispatch, harmless since it's already selected) — it is
        // NOT used for Tenant.SelectedModules dual-write, which is skipped entirely for
        // IsPlugin=true entries (see AppStoreModuleRegistry.ResolveModuleKey).
        new(PluginKeys.SsnitFiling, ModuleKeys.Payroll,
            ["EnablePluginSsnitFiling"],
            RequiresItemKey: "payroll",
            IsPlugin: true,
            DisableStep: "DisablePluginSsnitFiling"),
        new(PluginKeys.GhIpssExport, ModuleKeys.Payroll,
            ["EnablePluginGhIpssExport"],
            RequiresItemKey: "payroll",
            IsPlugin: true,
            DisableStep: "DisablePluginGhIpssExport"),
        new(PluginKeys.PayslipWhatsapp, ModuleKeys.Payroll,
            ["EnablePluginPayslipWhatsapp"],
            RequiresItemKey: "payroll",
            IsPlugin: true,
            DisableStep: "DisablePluginPayslipWhatsapp"),
        new(PluginKeys.FlexcubePosting, ModuleKeys.Payroll,
            ["EnablePluginFlexcubePosting"],
            RequiresItemKey: "payroll",
            IsPlugin: true,
            DisableStep: "DisablePluginFlexcubePosting"),
        new(PluginKeys.BiometricSync, ModuleKeys.CoreHR,
            ["EnablePluginBiometricSync"],
            RequiresItemKey: "attendance",
            IsPlugin: true,
            DisableStep: "DisablePluginBiometricSync"),
        new(PluginKeys.Esign, ModuleKeys.CoreHR,
            ["EnablePluginEsign"],
            RequiresItemKey: "core-hr",
            IsPlugin: true,
            DisableStep: "DisablePluginEsign"),
        new(PluginKeys.ChatNotify, ModuleKeys.Workflow,
            ["EnablePluginChatNotify"],
            RequiresItemKey: "workflows",
            IsPlugin: true,
            DisableStep: "DisablePluginChatNotify"),
        new(PluginKeys.SmsNotify, ModuleKeys.Workflow,
            ["EnablePluginSmsNotify"],
            RequiresItemKey: "workflows",
            IsPlugin: true,
            DisableStep: "DisablePluginSmsNotify"),
        new(PluginKeys.ConferenceNotify, ModuleKeys.Workflow,
            ["EnablePluginConferenceNotify"],
            RequiresItemKey: "workflows",
            IsPlugin: true,
            DisableStep: "DisablePluginConferenceNotify"),
        new(PluginKeys.WebhookNotify, ModuleKeys.Workflow,
            ["EnablePluginWebhookNotify"],
            RequiresItemKey: "workflows",
            IsPlugin: true,
            DisableStep: "DisablePluginWebhookNotify"),
    ];

    private static readonly SetupStepRoute[] SetupRoutes =
    [
        new("EnsureSubscription", "qimerp-iam-tenant-setup"),
        new("SyncSubscriptionModules", "qimerp-iam-tenant-setup"),
        new("CreateFirstEmployee", "qimerp-corehr-employee-tenant-setup"),
        new("AssignSuperAdminRole", "qimerp-iam-tenant-onboarding"),
        new("SetupIAMTenant", "qimerp-iam-tenant-onboarding"),
        new("SeedTenantReferenceData", "qimerp-iam-tenant-onboarding"),
        new("RepublishTenantReferenceSync", "qimerp-iam-tenant-onboarding"),
        new("SetupCoreHrTenant", "qimerp-corehr-employee-tenant-setup"),
        new("SetupPayrollTenant", "qimerp-payroll-tenant-setup"),
        new("SetupBenefitTenant", "qimerp-benefit-tenant-setup"),
        new("SetupRecruitmentTenant", "qimerp-recruitment-tenant-setup"),
        new("SetupSurveysTenant", "qimerp-surveys-tenant-setup"),
        new("SetupPayrollTenantConfig", "qimerp-payroll-tenant-setup"),
        new("SetupLeaveTenant", "qimerp-leave-tenant-setup"),
        new("InitialSetupCoreHrEmployee", "qimerp-corehr-employee-tenant-setup"),
        new("InitialSetupRecruitment", "qimerp-recruitment-tenant-setup"),
        new("InitialSetupBenefit", "qimerp-benefit-tenant-setup"),
        new("InitialSetupSurveys", "qimerp-surveys-tenant-setup"),
        new("InitialSetupEmployeeEngagement", "qimerp-engagement-tenant-setup"),
        new("InitialSetupLearning", "qimerp-learning-tenant-setup"),
        new("InitialSetupPerformance", "qimerp-performance-tenant-setup"),
        new("InitialSetupTalent", "qimerp-talent-tenant-setup"),
        new("InitialSetupWorkforcePlanning", "qimerp-workforceplanning-tenant-setup"),
        new("InitialSetupWorkflow", "qimerp-workflow-tenant-setup"),
        new("SetupAccountingGlTenant", "qimerp-accounting-gl-tenant-setup"),
        new("SetupAccountingApTenant", "qimerp-accounting-ap-tenant-setup"),
        new("SetupAccountingArTenant", "qimerp-accounting-ar-tenant-setup"),
        new("SetupBudgetPlanningTenant", "qimerp-accounting-budget-tenant-setup"),
        new("SetupCashManagementTenant", "qimerp-accounting-cash-tenant-setup"),
        new("SetupOperationsInventoryTenant", "qimerp-operations-inventory-tenant-setup"),
        new("SetupPosTenant", "qimerp-operations-pos-tenant-setup"),
        new("SetupOperationsProjectTenant", "qimerp-operations-project-tenant-setup"),
        new("SetupAttendanceTenant", "qimerp-corehr-employee-tenant-setup"),
        new("EnsureEmployeeInPayroll", "qimerp-payroll-tenant-setup"),
        new("EnsureEmployeeInLearning", "qimerp-learning-tenant-setup"),
        new("EnsureEmployeeInPerformance", "qimerp-performance-tenant-setup"),
        new("EnsureEmployeeInTalent", "qimerp-talent-tenant-setup"),
        new("EnsureEmployeeInWorkforcePlanning", "qimerp-workforceplanning-tenant-setup"),
        new("EnsureEmployeeInBenefit", "qimerp-benefit-tenant-setup"),
        new("EnsureEmployeeInEmployeeEngagement", "qimerp-engagement-tenant-setup"),
        new("EnsureEmployeeInLeave", "qimerp-leave-tenant-setup"),
        new("EnsureEmployeeInRecruitment", "qimerp-recruitment-tenant-setup"),
        new("EnsureEmployeeInSurveys", "qimerp-surveys-tenant-setup"),
        new("EnsureEmployeeInProject", "qimerp-operations-project-employee-sync"),

        // Plugin enable/disable — dispatched to the owning module's existing setup queue,
        // no new worker process.
        new("EnablePluginSsnitFiling", "qimerp-payroll-tenant-setup"),
        new("DisablePluginSsnitFiling", "qimerp-payroll-tenant-setup"),
        new("EnablePluginGhIpssExport", "qimerp-payroll-tenant-setup"),
        new("DisablePluginGhIpssExport", "qimerp-payroll-tenant-setup"),
        new("EnablePluginPayslipWhatsapp", "qimerp-payroll-tenant-setup"),
        new("DisablePluginPayslipWhatsapp", "qimerp-payroll-tenant-setup"),
        new("EnablePluginFlexcubePosting", "qimerp-payroll-tenant-setup"),
        new("DisablePluginFlexcubePosting", "qimerp-payroll-tenant-setup"),
        new("EnablePluginBiometricSync", "qimerp-corehr-employee-tenant-setup"),
        new("DisablePluginBiometricSync", "qimerp-corehr-employee-tenant-setup"),
        new("EnablePluginEsign", "qimerp-corehr-employee-tenant-setup"),
        new("DisablePluginEsign", "qimerp-corehr-employee-tenant-setup"),
        new("EnablePluginChatNotify", "qimerp-workflow-tenant-setup"),
        new("DisablePluginChatNotify", "qimerp-workflow-tenant-setup"),
        new("EnablePluginSmsNotify", "qimerp-workflow-tenant-setup"),
        new("DisablePluginSmsNotify", "qimerp-workflow-tenant-setup"),
        new("EnablePluginConferenceNotify", "qimerp-workflow-tenant-setup"),
        new("DisablePluginConferenceNotify", "qimerp-workflow-tenant-setup"),
        new("EnablePluginWebhookNotify", "qimerp-workflow-tenant-setup"),
        new("DisablePluginWebhookNotify", "qimerp-workflow-tenant-setup"),
        new("DisableWorkflowModule", "qimerp-workflow-tenant-setup"),
        new("SeedSampleCoreHr", "qimerp-corehr-employee-tenant-setup"),
        new("SeedSamplePayroll", "qimerp-payroll-tenant-setup"),
        new("SeedSampleLeave", "qimerp-leave-tenant-setup"),
        new("SeedSampleRecruitment", "qimerp-recruitment-tenant-setup"),
        new("SeedSamplePerformance", "qimerp-performance-tenant-setup"),
        new("SeedSampleSurveys", "qimerp-surveys-tenant-setup"),
        new("SeedSampleEmployeeEngagement", "qimerp-engagement-tenant-setup"),
        new("SeedSampleWorkforcePlanning", "qimerp-workforceplanning-tenant-setup"),
        new("SeedSampleWorkflows", "qimerp-workflow-tenant-setup"),
        new("SeedSampleInventory", "qimerp-operations-inventory-tenant-setup"),
        new("SeedSamplePos", "qimerp-operations-pos-tenant-setup"),
    ];

    private static readonly SyncSubscription[] Subscriptions =
    [
        new(SyncType.Employee, ModuleKeys.Payroll, "qimerp-payroll-employee-sync", "Payroll"),
        new(SyncType.Employee, ModuleKeys.Performance, "qimerp-performance-employee-sync", "Performance"),
        new(SyncType.Employee, ModuleKeys.Learning, "qimerp-learning-employee-sync", "Learning"),
        new(SyncType.Employee, ModuleKeys.Talent, "qimerp-talent-employee-sync", "Talent"),
        new(SyncType.Employee, ModuleKeys.WorkforcePlanning, "qimerp-workforceplanning-employee-sync", "WorkforcePlanning"),
        new(SyncType.Employee, ModuleKeys.Leave, "qimerp-leave-employee-sync", "Leave"),
        new(SyncType.Employee, ModuleKeys.Benefits, "qimerp-benefit-employee-sync", "Benefit"),
        new(SyncType.Employee, ModuleKeys.Recruitment, "qimerp-recruitment-employee-sync", "Recruitment"),
        new(SyncType.Employee, ModuleKeys.EmployeeEngagement, "qimerp-employeeengagement-employee-sync", "EmployeeEngagement"),
        new(SyncType.Employee, ModuleKeys.Surveys, "qimerp-surveys-employee-sync", "Surveys"),
        new(SyncType.Employee, null, "qimerp-iam-employee-sync", "IAM", RequiresModuleSelection: false),
        new(SyncType.Employee, ModuleKeys.Project, "qimerp-operations-project-employee-sync", "Project"),
        new(SyncType.Employee, null, "qimerp-iam-tenantbilling-employee-sync", "TenantBilling", RequiresModuleSelection: false),

        new(SyncType.AdminData, ModuleKeys.CoreHR, "qimerp-corehr-employee-admin-sync", "Employee"),
        new(SyncType.AdminData, ModuleKeys.Payroll, "qimerp-payroll-admin-sync", "Payroll"),
        new(SyncType.AdminData, ModuleKeys.CoreAccounting, "qimerp-accounting-admin-sync", "Accounting"),
        new(SyncType.AdminData, ModuleKeys.EmployeeEngagement, "qimerp-employeeengagement-admin-sync", "EmployeeEngagement"),
        new(SyncType.AdminData, ModuleKeys.Recruitment, "qimerp-recruitment-admin-sync", "Recruitment"),
        new(SyncType.AdminData, ModuleKeys.Surveys, "qimerp-surveys-admin-sync", "Surveys"),
        new(SyncType.AdminData, ModuleKeys.Leave, "qimerp-leave-admin-sync", "Leave"),
        new(SyncType.AdminData, ModuleKeys.Benefits, "qimerp-benefit-admin-sync", "Benefit"),
        new(SyncType.AdminData, ModuleKeys.Performance, "qimerp-performance-admin-sync", "Performance"),
        new(SyncType.AdminData, ModuleKeys.Project, "qimerp-operations-project-admin-sync", "Project"),

        new(SyncType.GlReference, ModuleKeys.Project, "qimerp-operations-project-gl-sync", "Project"),
        new(SyncType.GlReference, ModuleKeys.BudgetPlanning, "qimerp-accounting-budget-planning-gl-sync", "BudgetPlanning"),
        new(SyncType.GlReference, ModuleKeys.CashManagement, "qimerp-accounting-cash-management-gl-sync", "CashManagement"),

        new(SyncType.AssignmentChanged, ModuleKeys.Payroll, "qimerp-payroll-employee-sync", "Payroll"),

        new(SyncType.JournalEntryPosted, ModuleKeys.BudgetPlanning, "qimerp-accounting-budget-planning-je-sync", "BudgetPlanning"),

        new(SyncType.TenantReference, ModuleKeys.CoreHR, "qimerp-corehr-tenant-reference-sync", "CoreHR"),
        new(SyncType.TenantReference, ModuleKeys.Payroll, "qimerp-payroll-tenant-reference-sync", "Payroll"),
        new(SyncType.TenantReference, ModuleKeys.Leave, "qimerp-leave-tenant-reference-sync", "Leave"),
        new(SyncType.TenantReference, ModuleKeys.CoreAccounting, "qimerp-accounting-tenant-reference-sync", "Accounting"),

        new(SyncType.StockIssue, ModuleKeys.Inventory, "qimerp-inventory-stock-issue-sync", "Inventory"),
        new(SyncType.Customer, ModuleKeys.Inventory, "qimerp-inventory-customer-sync", "Inventory"),
        new(SyncType.Vendor, ModuleKeys.Inventory, "qimerp-inventory-vendor-sync", "Inventory"),
    ];

    private static readonly Dictionary<string, ModuleSyncDefinition> ByItemKey =
        Modules.ToDictionary(m => m.ItemKey, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> SetupQueueByStep =
        SetupRoutes.ToDictionary(r => r.StepName, r => r.TaskQueue, StringComparer.Ordinal);

    private static readonly Dictionary<string, ModuleSyncDefinition> ByModuleKey =
        Modules
            .Where(m => !m.IsPlugin)
            .ToDictionary(m => m.ModuleKey, m => m, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<ModuleSyncDefinition> GetAllModules() => Modules;

    public static ModuleSyncDefinition? TryGetModule(string itemKey) =>
        ByItemKey.GetValueOrDefault(itemKey);

    public static ModuleSyncDefinition GetRequiredModule(string itemKey) =>
        TryGetModule(itemKey) ?? throw new KeyNotFoundException($"No module definition for '{itemKey}'.");

    public static string? ResolveModuleKey(string itemKey) =>
        TryGetModule(itemKey)?.ModuleKey;

    public static string? TryResolveItemKey(string moduleKey) =>
        ByModuleKey.GetValueOrDefault(moduleKey)?.ItemKey;

    public static string[] ResolveSetupSteps(string itemKey) =>
        TryGetModule(itemKey)?.SetupSteps ?? [];

    public static string? ResolveEmployeeBackfillStep(string itemKey) =>
        TryGetModule(itemKey)?.EmployeeBackfillStep;

    public static string? ResolveAdminDataBackfillStep(string itemKey) =>
        TryGetModule(itemKey)?.AdminDataBackfillStep;

    public static string? ResolveDisableStep(string itemKey) =>
        TryGetModule(itemKey)?.DisableStep;

    public static string? ResolveSampleSeedStep(string itemKey) =>
        TryGetModule(itemKey)?.SampleSeedStep;

    public static bool IsPlugin(string itemKey) =>
        TryGetModule(itemKey)?.IsPlugin ?? false;

    public static IEnumerable<ModuleSyncDefinition> ResolvePrerequisites(string itemKey)
    {
        var def = TryGetModule(itemKey);
        if (def?.PrerequisiteItemKeys is null)
        {
            yield break;
        }

        foreach (var prereqKey in def.PrerequisiteItemKeys)
        {
            if (TryGetModule(prereqKey) is { } prereq)
            {
                yield return prereq;
            }
        }
    }

    public static string ResolveSetupStepQueue(string stepName) =>
        SetupQueueByStep.TryGetValue(stepName, out var queue)
            ? queue
            : throw new KeyNotFoundException($"No setup task queue for step '{stepName}'.");

    public static IReadOnlyList<SetupStepRoute> GetSetupStepRoutes() => SetupRoutes;

    public static IReadOnlyList<SyncSubscription> GetRuntimeSubscribers(SyncType type) =>
        Subscriptions.Where(s => s.Type == type).ToList();

    public static IReadOnlyList<SyncSubscription> FilterByInstalled(
        SyncType type,
        IReadOnlyList<string>? selectedModules) =>
        GetRuntimeSubscribers(type)
            .Where(s => !s.RequiresModuleSelection || ModuleGuard.IsSelected(selectedModules, s.ModuleKey!))
            .ToList();

    public static SyncSubscription? TryResolveEmployeeBackfillStep(string stepName) =>
        GetRuntimeSubscribers(SyncType.Employee)
            .FirstOrDefault(s => s.RequiresModuleSelection
                && string.Equals(stepName, $"EnsureEmployeeIn{s.ActivitySuffix}", StringComparison.Ordinal));

    public static SyncSubscription? TryResolveAdminBackfillStep(string stepName) =>
        GetRuntimeSubscribers(SyncType.AdminData)
            .FirstOrDefault(s => s.RequiresModuleSelection
                && s.ModuleKey != ModuleKeys.CoreHR
                && string.Equals(stepName, $"EnsureAdminDataIn{s.ActivitySuffix}", StringComparison.Ordinal));
}
