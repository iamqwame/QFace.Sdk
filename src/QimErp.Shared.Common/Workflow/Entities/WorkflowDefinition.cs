namespace QimErp.Shared.Common.Workflow.Entities;

public class WorkflowDefinition
{

    public List<WorkflowStep> Steps { get; set; } = [];


    public WorkflowNotificationSettings Notifications { get; set; } = new();
   

    public WorkflowEscalationSettings Escalation { get; set; } = new();

    public WorkflowAutoApprovalSettings AutoApproval { get; set; } = new();

    public WorkflowTimeoutSettings Timeout { get; set; } = new();
}

public class WorkflowStep
{
    public string StepCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Order { get; set; }
    public List<WorkflowApprover> RequiredApprovers { get; set; } = [];
    public int RequiredApprovals { get; set; } = 1;
    public int TimeoutDays { get; set; } = 5;
    public bool IsOptional { get; set; } = false;
    public List<WorkflowCondition> Conditions { get; set; } = [];
    public WorkflowStepAction OnApproval { get; set; } = new();
    public WorkflowStepAction OnRejection { get; set; } = new();
}

public class WorkflowApprover
{
    public string Type { get; set; } = ""; // "role", "department", "direct_employee", "rank", "ou"
    public string ValueId { get; set; } = "";
    public string Value { get; set; } = "";
}

public class WorkflowCondition
{
    public string Field { get; set; } = "";
    public WorkflowOperators Operator { get; set; } = WorkflowOperators.Equals;
    public string Value { get; set; } = "";
    public WorkflowLogicOperator Logic { get; set; } = WorkflowLogicOperator.And;
}

public class WorkflowStepAction
{
    public List<string> SendNotificationTo { get; set; } = [];
    public List<string> SendEmailTo { get; set; } = [];
    public string NextStepCode { get; set; } = "";
    public bool CompleteWorkflow { get; set; } = false;
}

public class WorkflowNotificationSettings
{
    public List<string> OnStart { get; set; } = [];
    public List<string> OnApproval { get; set; } = [];
    public List<string> OnRejection { get; set; } = [];
    public List<string> OnCompletion { get; set; } = [];
    public List<string> OnTimeout { get; set; } = [];
    public bool SendEmailNotifications { get; set; } = true;
    public bool SendSmsNotifications { get; set; } = false;
}

public class WorkflowEscalationSettings
{
    public bool Enabled { get; set; } = false;
    public int EscalateAfterDays { get; set; } = 7;
    public List<string> EscalateTo { get; set; } = [];
    public string EscalationMessage { get; set; } = "";
    public bool RepeatEscalation { get; set; } = false;
    public int RepeatIntervalDays { get; set; } = 3;
}

public class WorkflowAutoApprovalSettings
{
    public bool Enabled { get; set; } = false;
    public List<WorkflowCondition> Conditions { get; set; } = [];
    public string AutoApprovalReason { get; set; } = "Auto-approved based on predefined conditions";
    public List<string> NotifyOnAutoApproval { get; set; } = [];
}

public class WorkflowTimeoutSettings
{
    public int DefaultTimeoutDays { get; set; } = 7;
    public WorkflowTimeoutAction TimeoutAction { get; set; } = WorkflowTimeoutAction.Escalate;
    public string TimeoutReason { get; set; } = "Workflow timed out";
    public List<string> NotifyOnTimeout { get; set; } = [];
}
