namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for Employee entities across all modules.
/// Contains common properties and methods shared by module-specific Employee entities.
/// </summary>
public abstract class EmployeeBase : GuidAuditableEntity
{
    public string Code { get; protected set; } = string.Empty;
    public string FirstName { get; protected set; } = string.Empty;
    public string LastName { get; protected set; } = string.Empty;
    public string? MiddleName { get; protected set; }
    public string? Email { get; protected set; }
    public string? ProfilePicture { get; protected set; }

    /// <summary>"Male" | "Female" | "Unspecified" | null — synced from CoreHr's Employee.Gender.</summary>
    public string? Gender { get; protected set; }

    /// <summary>e.g. "Active", "Probation", "Terminated" — module-specific values, common shape.</summary>
    public string EmploymentStatus { get; protected set; } = "Active";

    public Guid? CurrentSupervisorId { get; protected set; }
    public string? CurrentSupervisorName { get; protected set; }
    public string? CurrentSupervisorCode { get; protected set; }
    public string? CurrentSupervisorTitle { get; protected set; }
    public string? CurrentSupervisorEmail { get; protected set; }
    public string? CurrentSupervisorPhone { get; protected set; }
    
    public Guid? CurrentOrganizationalUnitId { get; protected set; }
    public string? CurrentOrganizationalUnitName { get; protected set; }
    public string? CurrentOrganizationalUnitCode { get; protected set; }
    
    public Guid? CurrentJobTitleId { get; protected set; }
    public string? CurrentJobTitleName { get; protected set; }
    public string? CurrentJobTitleCode { get; protected set; }
    
    public Guid? CurrentStationId { get; protected set; }
    public string? CurrentStationName { get; protected set; }
    public string? CurrentStationCode { get; protected set; }
    
    public Guid? CurrentJobStatusId { get; protected set; }
    public string? CurrentJobStatusName { get; protected set; }
    public string? CurrentJobStatusCode { get; protected set; }
    
    public bool IsActive => DataStatus == DataState.Active;

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";

    public bool IsFemale => string.Equals(Gender, "Female", StringComparison.OrdinalIgnoreCase);

    public bool IsMale => string.Equals(Gender, "Male", StringComparison.OrdinalIgnoreCase);

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

    public EmployeeBase WithProfilePicture(string? profilePictureUrl)
    {
        ProfilePicture = profilePictureUrl;
        return this;
    }

    public EmployeeBase WithGender(string? gender)
    {
        Gender = gender;
        return this;
    }

    public EmployeeBase WithEmploymentStatus(string status)
    {
        EmploymentStatus = status;
        return this;
    }

    public EmployeeBase Activate()
    {
        AsActive();
        return this;
    }

    public new EmployeeBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}

