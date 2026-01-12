namespace QimErp.Shared.Common.Workflow.Enums;

public enum WorkflowStatus
{
    NotStarted = 0,
    InProgress = 1,
    Approved = 2,
    Rejected = 3,
    OnHold = 4,
    Cancelled = 5
}


public enum WorkflowTimeoutAction
{
    Escalate,
    Reject,
    Approve,
    Cancel
}

public enum WorkflowStepStatus
{
    Pending,
    InProgress,
    Approved,
    Rejected,
    Skipped,
    TimedOut
}

public enum WorkflowLogicOperator
{
    And,
    Or
}

public enum WorkflowOperators
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    In,
    NotIn
}





