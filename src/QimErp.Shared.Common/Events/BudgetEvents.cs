namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a budget is created
/// </summary>
public class BudgetCreatedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BudgetType { get; set; } // BudgetType as int
    public Guid FiscalYearId { get; set; }
    public Guid? FiscalPeriodId { get; set; }
    public Guid? OrganizationalUnitId { get; set; }
    public Guid? CostCenterId { get; set; }

    public BudgetCreatedEvent()
    {
    }

    public BudgetCreatedEvent(
        Guid budgetId,
        string budgetNumber,
        string name,
        int budgetType,
        Guid fiscalYearId,
        Guid? fiscalPeriodId,
        Guid? organizationalUnitId,
        Guid? costCenterId,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        BudgetNumber = budgetNumber;
        Name = name;
        BudgetType = budgetType;
        FiscalYearId = fiscalYearId;
        FiscalPeriodId = fiscalPeriodId;
        OrganizationalUnitId = organizationalUnitId;
        CostCenterId = costCenterId;
    }

    public static BudgetCreatedEvent Create(
        Guid budgetId,
        string budgetNumber,
        string name,
        int budgetType,
        Guid fiscalYearId,
        Guid? fiscalPeriodId,
        Guid? organizationalUnitId,
        Guid? costCenterId,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BudgetCreatedEvent(budgetId, budgetNumber, name, budgetType, fiscalYearId,
            fiscalPeriodId, organizationalUnitId, costCenterId, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a budget is updated
/// </summary>
public class BudgetUpdatedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalBudgetAmount { get; set; }

    public BudgetUpdatedEvent()
    {
    }

    public BudgetUpdatedEvent(
        Guid budgetId,
        string budgetNumber,
        string name,
        decimal totalBudgetAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        BudgetNumber = budgetNumber;
        Name = name;
        TotalBudgetAmount = totalBudgetAmount;
    }

    public static BudgetUpdatedEvent Create(
        Guid budgetId,
        string budgetNumber,
        string name,
        decimal totalBudgetAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BudgetUpdatedEvent(budgetId, budgetNumber, name, totalBudgetAmount, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a budget is approved
/// </summary>
public class BudgetApprovedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public decimal ApprovedAmount { get; set; }

    public BudgetApprovedEvent()
    {
    }

    public BudgetApprovedEvent(
        Guid budgetId,
        string budgetNumber,
        string approvedBy,
        decimal approvedAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        BudgetNumber = budgetNumber;
        ApprovedBy = approvedBy;
        ApprovedAmount = approvedAmount;
    }

    public static BudgetApprovedEvent Create(
        Guid budgetId,
        string budgetNumber,
        string approvedBy,
        decimal approvedAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BudgetApprovedEvent(budgetId, budgetNumber, approvedBy, approvedAmount, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a budget is activated
/// </summary>
public class BudgetActivatedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;

    public BudgetActivatedEvent()
    {
    }

    public BudgetActivatedEvent(
        Guid budgetId,
        string budgetNumber,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        BudgetNumber = budgetNumber;
    }

    public static BudgetActivatedEvent Create(
        Guid budgetId,
        string budgetNumber,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BudgetActivatedEvent(budgetId, budgetNumber, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a budget is closed
/// </summary>
public class BudgetClosedEvent : DomainEvent
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;

    public BudgetClosedEvent()
    {
    }

    public BudgetClosedEvent(
        Guid budgetId,
        string budgetNumber,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BudgetId = budgetId;
        BudgetNumber = budgetNumber;
    }

    public static BudgetClosedEvent Create(
        Guid budgetId,
        string budgetNumber,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BudgetClosedEvent(budgetId, budgetNumber, tenantId, userEmail, triggeredBy, userName);
    }
}
