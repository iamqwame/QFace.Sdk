namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a project is created or updated
/// </summary>
public class ProjectCreatedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal TotalBudget { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    public ProjectCreatedEvent()
    {
    }

    public ProjectCreatedEvent(
        Guid projectId,
        string code,
        string name,
        string? description,
        string status,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        decimal totalBudget,
        string currencyCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ProjectId = projectId;
        Code = code;
        Name = name;
        Description = description;
        Status = status;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        TotalBudget = totalBudget;
        CurrencyCode = currencyCode;
    }

    public static ProjectCreatedEvent Create(
        Guid projectId,
        string code,
        string name,
        string? description,
        string status,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        decimal totalBudget,
        string currencyCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectCreatedEvent(projectId, code, name, description, status, plannedStartDate, plannedEndDate, totalBudget, currencyCode, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project is updated
/// </summary>
public class ProjectUpdatedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal TotalBudget { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    public ProjectUpdatedEvent()
    {
    }

    public ProjectUpdatedEvent(
        Guid projectId,
        string code,
        string name,
        string? description,
        string status,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        decimal totalBudget,
        string currencyCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ProjectId = projectId;
        Code = code;
        Name = name;
        Description = description;
        Status = status;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        TotalBudget = totalBudget;
        CurrencyCode = currencyCode;
    }

    public static ProjectUpdatedEvent Create(
        Guid projectId,
        string code,
        string name,
        string? description,
        string status,
        DateTime plannedStartDate,
        DateTime plannedEndDate,
        decimal totalBudget,
        string currencyCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectUpdatedEvent(projectId, code, name, description, status, plannedStartDate, plannedEndDate, totalBudget, currencyCode, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project is deleted or deactivated
/// </summary>
public class ProjectDeletedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ProjectDeletedEvent()
    {
    }

    public ProjectDeletedEvent(
        Guid projectId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ProjectId = projectId;
        Code = code;
        Name = name;
    }

    public static ProjectDeletedEvent Create(
        Guid projectId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectDeletedEvent(projectId, code, name, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project expenditure is created
/// </summary>
public class ProjectExpenditureCreatedEvent : DomainEvent
{
    public Guid ExpenditureId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ExpenditureNumber { get; set; } = string.Empty;
    public DateTime ExpenditureDate { get; set; }
    public decimal TotalCost { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public ProjectExpenditureCreatedEvent()
    {
    }

    public ProjectExpenditureCreatedEvent(
        Guid expenditureId,
        Guid projectId,
        string projectNumber,
        string expenditureNumber,
        DateTime expenditureDate,
        decimal totalCost,
        string currencyCode,
        string source,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ExpenditureId = expenditureId;
        ProjectId = projectId;
        ProjectNumber = projectNumber;
        ExpenditureNumber = expenditureNumber;
        ExpenditureDate = expenditureDate;
        TotalCost = totalCost;
        CurrencyCode = currencyCode;
        Source = source;
    }

    public static ProjectExpenditureCreatedEvent Create(
        Guid expenditureId,
        Guid projectId,
        string projectNumber,
        string expenditureNumber,
        DateTime expenditureDate,
        decimal totalCost,
        string currencyCode,
        string source,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectExpenditureCreatedEvent(expenditureId, projectId, projectNumber, expenditureNumber, expenditureDate, totalCost, currencyCode, source, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project time entry is created
/// </summary>
public class ProjectTimeEntryCreatedEvent : DomainEvent
{
    public Guid TimeEntryId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalBillableAmount { get; set; }

    public ProjectTimeEntryCreatedEvent()
    {
    }

    public ProjectTimeEntryCreatedEvent(
        Guid timeEntryId,
        Guid projectId,
        string projectNumber,
        DateTime workDate,
        decimal hours,
        decimal totalCost,
        decimal totalBillableAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        TimeEntryId = timeEntryId;
        ProjectId = projectId;
        ProjectNumber = projectNumber;
        WorkDate = workDate;
        Hours = hours;
        TotalCost = totalCost;
        TotalBillableAmount = totalBillableAmount;
    }

    public static ProjectTimeEntryCreatedEvent Create(
        Guid timeEntryId,
        Guid projectId,
        string projectNumber,
        DateTime workDate,
        decimal hours,
        decimal totalCost,
        decimal totalBillableAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectTimeEntryCreatedEvent(timeEntryId, projectId, projectNumber, workDate, hours, totalCost, totalBillableAmount, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project budget is updated
/// </summary>
public class ProjectBudgetUpdatedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public Guid ProjectId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public int Version { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsBaseline { get; set; }

    public ProjectBudgetUpdatedEvent()
    {
    }

    public ProjectBudgetUpdatedEvent(
        Guid budgetId,
        Guid projectId,
        string budgetNumber,
        int version,
        decimal totalAmount,
        bool isBaseline,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        ProjectId = projectId;
        BudgetNumber = budgetNumber;
        Version = version;
        TotalAmount = totalAmount;
        IsBaseline = isBaseline;
    }

    public static ProjectBudgetUpdatedEvent Create(
        Guid budgetId,
        Guid projectId,
        string budgetNumber,
        int version,
        decimal totalAmount,
        bool isBaseline,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectBudgetUpdatedEvent(budgetId, projectId, budgetNumber, version, totalAmount, isBaseline, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a project milestone is completed
/// </summary>
public class ProjectMilestoneCompletedEvent : DomainEvent
{
    public Guid MilestoneId { get; set; }
    public Guid ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public DateTime ActualDate { get; set; }
    public bool IsBillingMilestone { get; set; }
    public decimal? BillingAmount { get; set; }

    public ProjectMilestoneCompletedEvent()
    {
    }

    public ProjectMilestoneCompletedEvent(
        Guid milestoneId,
        Guid projectId,
        string milestoneName,
        DateTime actualDate,
        bool isBillingMilestone,
        decimal? billingAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        MilestoneId = milestoneId;
        ProjectId = projectId;
        MilestoneName = milestoneName;
        ActualDate = actualDate;
        IsBillingMilestone = isBillingMilestone;
        BillingAmount = billingAmount;
    }

    public static ProjectMilestoneCompletedEvent Create(
        Guid milestoneId,
        Guid projectId,
        string milestoneName,
        DateTime actualDate,
        bool isBillingMilestone,
        decimal? billingAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectMilestoneCompletedEvent(milestoneId, projectId, milestoneName, actualDate, isBillingMilestone, billingAmount, tenantId, userEmail, triggeredBy, userName);
    }
}
