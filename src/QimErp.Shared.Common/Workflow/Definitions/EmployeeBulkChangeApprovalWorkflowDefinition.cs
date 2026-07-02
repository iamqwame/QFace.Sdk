namespace QimErp.Shared.Common.Workflow.Definitions;

/// <summary>
/// Bulk employee change request approval: submitter's manager, then HR finalize.
/// Applies to <see cref="EmployeeChangeRequest"/> bulk submissions (OU, station, manager, title, status).
/// </summary>
public static class EmployeeBulkChangeApprovalWorkflowDefinition
{
    public const string WorkflowCode = "EMPLOYEE_BULK_CHANGE_APPROVAL";

    public const string HrDepartmentName = EmployeeCreateApprovalWorkflowDefinition.HrDepartmentName;

    public static WorkflowDefinition Create() =>
        new()
        {
            Steps =
            [
                new WorkflowStep
                {
                    StepCode = "CURRENT_MANAGER_REVIEW",
                    Name = "Current Manager Review",
                    Description = "Line manager reviews the bulk change request",
                    Order = 1,
                    RequiredApprovers = [DirectManager()],
                    RequiredApprovals = 1,
                    TimeoutDays = 2,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction
                    {
                        NextStepCode = "HR_FINALIZE",
                        CompleteWorkflow = false
                    },
                    OnRejection = new WorkflowStepAction
                    {
                        NextStepCode = "complete",
                        CompleteWorkflow = true
                    }
                },
                new WorkflowStep
                {
                    StepCode = "HR_FINALIZE",
                    Name = "HR Finalize Bulk Change",
                    Description = "HR approves and applies the bulk assignment change",
                    Order = 2,
                    RequiredApprovers = [Dept(HrDepartmentName)],
                    RequiredApprovals = 1,
                    TimeoutDays = 2,
                    BypassRoles = [],
                    OnApproval = new WorkflowStepAction
                    {
                        NextStepCode = "complete",
                        CompleteWorkflow = true
                    },
                    OnRejection = new WorkflowStepAction
                    {
                        NextStepCode = "complete",
                        CompleteWorkflow = true
                    }
                }
            ],
            Notifications = new WorkflowNotificationSettings { SendEmailNotifications = true },
            Timeout = new WorkflowTimeoutSettings { DefaultTimeoutDays = 5 }
        };

    private static WorkflowApprover Dept(string name) =>
        new() { Type = "department", ValueId = "people_operations", Value = name };

    private static WorkflowApprover DirectManager() =>
        new() { Type = "direct_manager", ValueId = "direct_manager", Value = "Line Manager" };
}
