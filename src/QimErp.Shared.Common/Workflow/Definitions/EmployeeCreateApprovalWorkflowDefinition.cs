using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Workflow.Definitions;

/// <summary>
/// Standard four-step new-hire approval chain. Each step is owned by a functional area —
/// not a platform super-admin. Department names must match tenant organizational units.
/// </summary>
public static class EmployeeCreateApprovalWorkflowDefinition
{
    public const string WorkflowCode = "employee-create-approval";

    /// <summary>HR / people team (e.g. techlabs: People Operations).</summary>
    public const string HrDepartmentName = "People Operations";

    /// <summary>IT / platform team (e.g. techlabs: Infrastructure &amp; Platform).</summary>
    public const string ItDepartmentName = "Infrastructure & Platform";

    /// <summary>Salon / beauty chain HR back-office (matches <see cref="SalonBeautyDepartmentNames.Hr"/>).</summary>
    public const string SalonHrDepartmentName = SalonBeautyDepartmentNames.Hr;

    /// <summary>Salon systems, POS, and scheduling team (matches <see cref="SalonBeautyDepartmentNames.Operations"/>).</summary>
    public const string SalonItDepartmentName = SalonBeautyDepartmentNames.Operations;

    public static class SalonBeautyDepartmentNames
    {
        public const string Hr = "Salon Administration";
        public const string Operations = "Salon Operations";
    }

    public static WorkflowDefinition Create() =>
        Create(HrDepartmentName, ItDepartmentName);

    public static WorkflowDefinition CreateForSalonBeauty() =>
        Create(SalonHrDepartmentName, SalonItDepartmentName);

    public static WorkflowDefinition Create(string hrDepartmentName, string? itDepartmentName = null)
    {
        var hr = string.IsNullOrWhiteSpace(hrDepartmentName) ? HrDepartmentName : hrDepartmentName.Trim();
        var it = string.IsNullOrWhiteSpace(itDepartmentName) ? ItDepartmentName : itDepartmentName.Trim();

        return new WorkflowDefinition
        {
            Steps =
            [
                new WorkflowStep
                {
                    StepCode = "HR_REVIEW",
                    Name = "HR Review",
                    Description = "HR team reviews new employee details and supporting documents",
                    Order = 1,
                    RequiredApprovers = [Dept(hr)],
                    RequiredApprovals = 1,
                    TimeoutDays = 3,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction { NextStepCode = "DEPT_HEAD_APPROVAL" },
                    OnRejection = new WorkflowStepAction { NextStepCode = "complete", CompleteWorkflow = true }
                },
                new WorkflowStep
                {
                    StepCode = "DEPT_HEAD_APPROVAL",
                    Name = "Department Head Approval",
                    Description = "Hiring manager / department head approves the hire",
                    Order = 2,
                    RequiredApprovers = [DirectManager()],
                    RequiredApprovals = 1,
                    TimeoutDays = 5,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction { NextStepCode = "IT_SETUP" },
                    OnRejection = new WorkflowStepAction { NextStepCode = "complete", CompleteWorkflow = true }
                },
                new WorkflowStep
                {
                    StepCode = "IT_SETUP",
                    Name = "IT Setup",
                    Description = "IT provisions accounts, equipment and system access",
                    Order = 3,
                    RequiredApprovers = [Dept(it)],
                    RequiredApprovals = 1,
                    TimeoutDays = 2,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction { NextStepCode = "FINAL_VERIFICATION" }
                },
                new WorkflowStep
                {
                    StepCode = "FINAL_VERIFICATION",
                    Name = "Final HR Verification",
                    Description = "HR completes final checks before activation and onboarding",
                    Order = 4,
                    RequiredApprovers = [Dept(hr)],
                    RequiredApprovals = 1,
                    TimeoutDays = 2,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction { NextStepCode = "complete", CompleteWorkflow = true }
                }
            ],
            Notifications = new WorkflowNotificationSettings { SendEmailNotifications = true },
            Timeout = new WorkflowTimeoutSettings { DefaultTimeoutDays = 5 }
        };
    }

    /// <summary>
    /// Resolves a workflow definition from an optional preset variant or explicit department names.
    /// </summary>
    public static WorkflowDefinition Resolve(string? variant = null, string? hrDepartment = null, string? itDepartment = null) =>
        variant?.Trim().ToLowerInvariant() switch
        {
            "salon" or "salon-beauty" or "beauty" => CreateForSalonBeauty(),
            _ when !string.IsNullOrWhiteSpace(hrDepartment) => Create(hrDepartment, itDepartment),
            _ => Create()
        };

    private static WorkflowApprover Dept(string name) =>
        new() { Type = "department", ValueId = "", Value = name };

    private static WorkflowApprover DirectManager() =>
        new() { Type = "direct_manager", ValueId = "", Value = "Hiring Manager" };
}
