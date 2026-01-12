namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for Employee entities across all modules.
/// Contains common properties and methods shared by module-specific Employee entities.
/// </summary>
public abstract class EmployeeBase : GuidAuditableEntity
{
    // Basic Information
    public string Code { get; protected set; } = string.Empty;
    public string FirstName { get; protected set; } = string.Empty;
    public string LastName { get; protected set; } = string.Empty;
    public string? MiddleName { get; protected set; }
    public string? Email { get; protected set; }
    public string? ProfilePicture { get; protected set; }
    
    // Current Manager/Supervisor relationship
    public Guid? CurrentSupervisorId { get; protected set; }
    public string? CurrentSupervisorName { get; protected set; }
    public string? CurrentSupervisorCode { get; protected set; }
    public string? CurrentSupervisorTitle { get; protected set; }
    public string? CurrentSupervisorEmail { get; protected set; }
    public string? CurrentSupervisorPhone { get; protected set; }
    
    // Current Organizational Unit (for filtering/grouping)
    public Guid? CurrentOrganizationalUnitId { get; protected set; }
    public string? CurrentOrganizationalUnitName { get; protected set; }
    public string? CurrentOrganizationalUnitCode { get; protected set; }
    
    // Current Job Title
    public Guid? CurrentJobTitleId { get; protected set; }
    public string? CurrentJobTitleName { get; protected set; }
    public string? CurrentJobTitleCode { get; protected set; }
    
    // Current Station (Location)
    public Guid? CurrentStationId { get; protected set; }
    public string? CurrentStationName { get; protected set; }
    public string? CurrentStationCode { get; protected set; }
    
    // Current Job Status
    public Guid? CurrentJobStatusId { get; protected set; }
    public string? CurrentJobStatusName { get; protected set; }
    public string? CurrentJobStatusCode { get; protected set; }
    
    // Computed Properties
    public bool IsActive => DataStatus == DataState.Active;

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";

    protected EmployeeBase() { }

    protected EmployeeBase(
        Guid id,
        string code,
        string firstName,
        string lastName,
        string? middleName = null,
        string? email = null,
        string? profilePicture = null)
    {
        Id = id;
        Code = code;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Email = email;
        ProfilePicture = profilePicture;
        AsActive();
    }

    /// <summary>
    /// Updates basic employee information (name and email)
    /// </summary>
    public EmployeeBase UpdateBasicInfo(
        string firstName,
        string lastName,
        string? middleName = null,
        string? email = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Email = email;
        return this;
    }

    /// <summary>
    /// Updates current supervisor/manager information
    /// </summary>
    public EmployeeBase UpdateCurrentSupervisor(
        Guid? supervisorId = null,
        string? supervisorName = null,
        string? supervisorCode = null,
        string? supervisorTitle = null,
        string? supervisorEmail = null,
        string? supervisorPhone = null)
    {
        CurrentSupervisorId = supervisorId;
        CurrentSupervisorName = supervisorName;
        CurrentSupervisorCode = supervisorCode;
        CurrentSupervisorTitle = supervisorTitle;
        CurrentSupervisorEmail = supervisorEmail;
        CurrentSupervisorPhone = supervisorPhone;
        return this;
    }

    /// <summary>
    /// Updates current organizational unit information
    /// </summary>
    public EmployeeBase UpdateCurrentOrganizationalUnit(
        Guid? organizationalUnitId = null,
        string? organizationalUnitName = null,
        string? organizationalUnitCode = null)
    {
        CurrentOrganizationalUnitId = organizationalUnitId;
        CurrentOrganizationalUnitName = organizationalUnitName;
        CurrentOrganizationalUnitCode = organizationalUnitCode;
        return this;
    }

    /// <summary>
    /// Updates current job title information
    /// </summary>
    public EmployeeBase UpdateCurrentJobTitle(
        Guid? jobTitleId = null,
        string? jobTitleName = null,
        string? jobTitleCode = null)
    {
        CurrentJobTitleId = jobTitleId;
        CurrentJobTitleName = jobTitleName;
        CurrentJobTitleCode = jobTitleCode;
        return this;
    }

    /// <summary>
    /// Updates current station information
    /// </summary>
    public EmployeeBase UpdateCurrentStation(
        Guid? stationId = null,
        string? stationName = null,
        string? stationCode = null)
    {
        CurrentStationId = stationId;
        CurrentStationName = stationName;
        CurrentStationCode = stationCode;
        return this;
    }

    /// <summary>
    /// Updates current job status information
    /// </summary>
    public EmployeeBase UpdateCurrentJobStatus(
        Guid? jobStatusId = null,
        string? jobStatusName = null,
        string? jobStatusCode = null)
    {
        CurrentJobStatusId = jobStatusId;
        CurrentJobStatusName = jobStatusName;
        CurrentJobStatusCode = jobStatusCode;
        return this;
    }

    /// <summary>
    /// Updates profile picture URL
    /// </summary>
    public EmployeeBase WithProfilePicture(string? profilePictureUrl)
    {
        ProfilePicture = profilePictureUrl;
        return this;
    }

    /// <summary>
    /// Activates the employee
    /// </summary>
    public EmployeeBase Activate()
    {
        AsActive();
        return this;
    }

    /// <summary>
    /// Deactivates the employee
    /// </summary>
    public new EmployeeBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}

