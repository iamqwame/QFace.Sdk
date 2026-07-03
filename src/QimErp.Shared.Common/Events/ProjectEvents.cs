namespace QimErp.Shared.Common.Events;

public class ProjectCreatedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
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

    public static ProjectCreatedEvent Create(
        Guid projectId,
        string projectNumber,
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
        return new ProjectCreatedEvent
        {
            ProjectId = projectId,
            ProjectNumber = projectNumber,
            Name = name,
            Description = description,
            Status = status,
            PlannedStartDate = plannedStartDate,
            PlannedEndDate = plannedEndDate,
            TotalBudget = totalBudget,
            CurrencyCode = currencyCode,
            TenantId = tenantId,
            UserEmail = userEmail,
            TriggeredBy = triggeredBy,
            UserName = userName
        };
    }
}

public class ProjectUpdatedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
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

    public static ProjectUpdatedEvent Create(
        Guid projectId,
        string projectNumber,
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
        return new ProjectUpdatedEvent
        {
            ProjectId = projectId,
            ProjectNumber = projectNumber,
            Name = name,
            Description = description,
            Status = status,
            PlannedStartDate = plannedStartDate,
            PlannedEndDate = plannedEndDate,
            TotalBudget = totalBudget,
            CurrencyCode = currencyCode,
            TenantId = tenantId,
            UserEmail = userEmail,
            TriggeredBy = triggeredBy,
            UserName = userName
        };
    }
}

public class ProjectDeletedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ProjectDeletedEvent()
    {
    }

    public static ProjectDeletedEvent Create(
        Guid projectId,
        string projectNumber,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectDeletedEvent
        {
            ProjectId = projectId,
            ProjectNumber = projectNumber,
            Name = name,
            TenantId = tenantId,
            UserEmail = userEmail,
            TriggeredBy = triggeredBy,
            UserName = userName
        };
    }
}

/// <summary>
/// Published when Project generates a bill for customer invoicing. AR consumes via
/// <see cref="Activities.Operations.IProjectBillSyncWorkflow"/>.
/// </summary>
public class ProjectBillGeneratedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public List<ProjectBillLineData> Lines { get; set; } = [];

    public ProjectBillGeneratedEvent()
    {
    }

    public ProjectBillGeneratedEvent(
        Guid projectId,
        string projectNumber,
        string projectName,
        Guid billId,
        string billNumber,
        Guid customerId,
        string customerName,
        DateTime billDate,
        string currencyCode,
        decimal grandTotal,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ProjectId = projectId;
        ProjectNumber = projectNumber;
        ProjectName = projectName;
        BillId = billId;
        BillNumber = billNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        BillDate = billDate;
        CurrencyCode = currencyCode;
        GrandTotal = grandTotal;
    }

    public static ProjectBillGeneratedEvent Create(
        Guid projectId,
        string projectNumber,
        string projectName,
        Guid billId,
        string billNumber,
        Guid customerId,
        string customerName,
        DateTime billDate,
        string currencyCode,
        decimal grandTotal,
        string tenantId,
        IEnumerable<ProjectBillLineData> lines,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ProjectBillGeneratedEvent(
            projectId, projectNumber, projectName, billId, billNumber,
            customerId, customerName, billDate, currencyCode, grandTotal, tenantId,
            userEmail, triggeredBy, userName)
        {
            Lines = lines.ToList()
        };
    }
}

public class ProjectBillLineData
{
    public Guid LineId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskNumber { get; set; }
    public Guid? ExpenditureId { get; set; }
    public Guid? TimeEntryId { get; set; }
}
