namespace QimErp.Shared.Common.Workflow.Definitions;

/// <summary>
/// Two-step employee OU transfer approval: current manager, then HR finalize.
/// </summary>
public static class EmployeeTransferApprovalWorkflowDefinition
{
    public const string WorkflowCode = "EMPLOYEE_TRANSFER_APPROVAL";

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
                    Description = "Current line manager approves the transfer",
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
                    Name = "HR Finalize Change",
                    Description = "HR records the approved transfer",
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
