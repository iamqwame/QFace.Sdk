namespace QimErp.Shared.Common.Events;


public class EmployeeChangedEvent : DomainEvent
{
    public string EmployeeId { get; set; } = string.Empty;
    public string? EmployeeNo { get; set; }
    public string? Code { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; }
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? JobTitle { get; set; }
    public Guid? JobTitleId { get; set; }
    public string? JobTitleCode { get; set; }
    public string? ProfilePicture { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ReferenceCode { get; set; }
    public bool IsActive { get; set; }
    public Guid? OrganizationalUnitId { get; set; }
    public string? OrganizationalUnitName { get; set; }
    public string? OrganizationalUnitCode { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationCode { get; set; }
    
    // Supervisor fields
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
    public string? SupervisorCode { get; set; }
    public string? SupervisorTitle { get; set; }
    public string? SupervisorEmail { get; set; }
    public string? SupervisorPhone { get; set; }
    
    // Job Status fields
    public Guid? JobStatusId { get; set; }
    public string? JobStatusName { get; set; }
    public string? JobStatusCode { get; set; }

    public bool SendInvitation { get; set; }

    public EmployeeChangedEvent()
    {
    }

    private EmployeeChangedEvent(string email, string tenantId, string userEmail, string? triggeredBy = null, string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        Email = email;
    }

   
    public static EmployeeChangedEvent Create(string email, string firstName, string lastName,
        string tenantId, string userEmail, string? triggeredBy = null, string? userName = null)
    {
        return new EmployeeChangedEvent(email, tenantId, userEmail, triggeredBy, userName)
        {
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };
    }
    
    public EmployeeChangedEvent WithEmployeeId(string id) { EmployeeId = id; return this; }
    public EmployeeChangedEvent WithEmployeeNo(string? no) { EmployeeNo = no; return this; }
    public EmployeeChangedEvent WithCode(string? code) { Code = code; return this; }
    public EmployeeChangedEvent WithMiddleName(string? middle) { MiddleName = middle; return this; }
    public EmployeeChangedEvent WithJobTitle(string? jobTitle) { JobTitle = jobTitle; return this; }
    public EmployeeChangedEvent WithJobTitle(Guid? jobTitleId, string? jobTitleName, string? jobTitleCode = null)
    {
        JobTitleId = jobTitleId;
        JobTitle = jobTitleName;
        JobTitleCode = jobTitleCode;
        return this;
    }
    public EmployeeChangedEvent WithProfilePicture(string? picture) { ProfilePicture = picture; return this; }
    public EmployeeChangedEvent WithPhoneNumber(string? phone) { PhoneNumber = phone; return this; }
    public EmployeeChangedEvent WithReferenceCode(string? reference) { ReferenceCode = reference; return this; }
    public EmployeeChangedEvent Active(bool isActive = true) { IsActive = isActive; return this; }
    
    public EmployeeChangedEvent WithOrganizationalUnit(Guid? id, string? name, string? code = null) 
    { 
        OrganizationalUnitId = id; 
        OrganizationalUnitName = name; 
        OrganizationalUnitCode = code; 
        return this; 
    }
    
    public EmployeeChangedEvent WithLocation(Guid? id, string? name, string? code = null) 
    { 
        LocationId = id; 
        LocationName = name; 
        LocationCode = code; 
        return this; 
    }
    
    public EmployeeChangedEvent WithSupervisor(
        Guid? supervisorId = null,
        string? supervisorName = null,
        string? supervisorCode = null,
        string? supervisorTitle = null,
        string? supervisorEmail = null,
        string? supervisorPhone = null)
    {
        SupervisorId = supervisorId;
        SupervisorName = supervisorName;
        SupervisorCode = supervisorCode;
        SupervisorTitle = supervisorTitle;
        SupervisorEmail = supervisorEmail;
        SupervisorPhone = supervisorPhone;
        return this;
    }
    
    public EmployeeChangedEvent WithJobStatus(Guid? jobStatusId = null, string? jobStatusName = null, string? jobStatusCode = null)
    {
        JobStatusId = jobStatusId;
        JobStatusName = jobStatusName;
        JobStatusCode = jobStatusCode;
        return this;
    }
    
    public EmployeeChangedEvent WithInvitation(bool sendInvitation = true) { SendInvitation = sendInvitation; return this; }
}


public class EmployeeDeletedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }

    public EmployeeDeletedEvent()
    {
    }

    public EmployeeDeletedEvent(
        Guid employeeId,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        EmployeeId = employeeId;
    }

    public static EmployeeDeletedEvent Create(
        Guid employeeId,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new EmployeeDeletedEvent(employeeId, tenantId, userEmail, triggeredBy, userName);
    }
}

