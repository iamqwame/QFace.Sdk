namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a job title's basic information is updated
/// </summary>
public class JobTitleUpdatedEvent : DomainEvent
{
    public Guid JobTitleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public JobTitleUpdatedEvent()
    {
    }

    public JobTitleUpdatedEvent(
        Guid jobTitleId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        JobTitleId = jobTitleId;
        Name = name;
        Code = code;
    }

    public static JobTitleUpdatedEvent Create(
        Guid jobTitleId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new JobTitleUpdatedEvent(jobTitleId, name, code, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a job title is deleted
/// </summary>
public class JobTitleDeletedEvent : DomainEvent
{
    public Guid JobTitleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public JobTitleDeletedEvent()
    {
    }

    public JobTitleDeletedEvent(
        Guid jobTitleId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        JobTitleId = jobTitleId;
        Name = name;
        Code = code;
    }

    public static JobTitleDeletedEvent Create(
        Guid jobTitleId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new JobTitleDeletedEvent(jobTitleId, name, code, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a station's basic information is updated
/// </summary>
public class StationUpdatedEvent : DomainEvent
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public StationUpdatedEvent()
    {
    }

    public StationUpdatedEvent(
        Guid stationId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        StationId = stationId;
        Name = name;
        Code = code;
    }

    public static StationUpdatedEvent Create(
        Guid stationId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new StationUpdatedEvent(stationId, name, code, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a station is deleted
/// </summary>
public class StationDeletedEvent : DomainEvent
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public StationDeletedEvent()
    {
    }

    public StationDeletedEvent(
        Guid stationId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        StationId = stationId;
        Name = name;
        Code = code;
    }

    public static StationDeletedEvent Create(
        Guid stationId,
        string name,
        string? code,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new StationDeletedEvent(stationId, name, code, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when an organizational unit's information is updated
/// Note: Type and BudgetPeriod are stored as int to avoid cross-module enum dependencies
/// </summary>
public class OrganizationalUnitUpdatedEvent : DomainEvent
{
    public Guid OrganizationalUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int Type { get; set; } // OrganizationalUnitType as int
    public string? Location { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? BudgetCurrency { get; set; }
    public int BudgetPeriod { get; set; } // BudgetPeriod as int
    public string? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerCode { get; set; }
    public string? ManagerEmail { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Extension { get; set; }
    public string? CostCenter { get; set; }
    public string? Purpose { get; set; }

    public OrganizationalUnitUpdatedEvent()
    {
    }

    public OrganizationalUnitUpdatedEvent(
        Guid organizationalUnitId,
        string name,
        string? code,
        int type,
        string? location,
        decimal? budgetMin,
        decimal? budgetMax,
        string? budgetCurrency,
        int budgetPeriod,
        string? managerId,
        string? managerName,
        string? managerCode,
        string? managerEmail,
        string? phone,
        string? email,
        string? extension,
        string? costCenter,
        string? purpose,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        OrganizationalUnitId = organizationalUnitId;
        Name = name;
        Code = code;
        Type = type;
        Location = location;
        BudgetMin = budgetMin;
        BudgetMax = budgetMax;
        BudgetCurrency = budgetCurrency;
        BudgetPeriod = budgetPeriod;
        ManagerId = managerId;
        ManagerName = managerName;
        ManagerCode = managerCode;
        ManagerEmail = managerEmail;
        Phone = phone;
        Email = email;
        Extension = extension;
        CostCenter = costCenter;
        Purpose = purpose;
    }

    public static OrganizationalUnitUpdatedEvent Create(
        Guid organizationalUnitId,
        string name,
        string? code,
        int type,
        string? location,
        decimal? budgetMin,
        decimal? budgetMax,
        string? budgetCurrency,
        int budgetPeriod,
        string? managerId,
        string? managerName,
        string? managerCode,
        string? managerEmail,
        string? phone,
        string? email,
        string? extension,
        string? costCenter,
        string? purpose,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new OrganizationalUnitUpdatedEvent(
            organizationalUnitId,
            name,
            code,
            type,
            location,
            budgetMin,
            budgetMax,
            budgetCurrency,
            budgetPeriod,
            managerId,
            managerName,
            managerCode,
            managerEmail,
            phone,
            email,
            extension,
            costCenter,
            purpose,
            tenantId,
            userEmail,
            triggeredBy,
            userName);
    }
}

/// <summary>
/// Domain event fired when an organizational unit is deleted
/// </summary>
public class OrganizationalUnitDeletedEvent : DomainEvent
{
    public Guid OrganizationalUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int Type { get; set; } // OrganizationalUnitType as int

    public OrganizationalUnitDeletedEvent()
    {
    }

    public OrganizationalUnitDeletedEvent(
        Guid organizationalUnitId,
        string name,
        string? code,
        int type,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        OrganizationalUnitId = organizationalUnitId;
        Name = name;
        Code = code;
        Type = type;
    }

    public static OrganizationalUnitDeletedEvent Create(
        Guid organizationalUnitId,
        string name,
        string? code,
        int type,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new OrganizationalUnitDeletedEvent(organizationalUnitId, name, code, type, tenantId, userEmail, triggeredBy, userName);
    }
}

