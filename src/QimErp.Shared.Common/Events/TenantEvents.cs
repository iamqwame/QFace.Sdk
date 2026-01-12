namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a new tenant is registered
/// This event is published by Auth module after successful tenant registration
/// and consumed by HR modules to perform initial setup/seeding
/// </summary>
public class TenantRegisteredEvent : DomainEvent
{
    public string Company { get; set; }
    public string Domain { get; set; } 
    public string WorkEmail { get; set; } 
    public string? PersonalEmail { get; set; }
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
    public string Country { get; set; } 
    public string EmployeeCount { get; set; } 
    public string PreferredDatabase { get; set; }
    public string? CompanyType { get; set; }
    public string? IndustryType { get; set; }


    public TenantRegisteredEvent()
    {
    }

    public TenantRegisteredEvent(
        string tenantId,
        string company,
        string domain,
        string workEmail,
        string firstName,
        string lastName,
        string country,
        string employeeCount,
        string preferredDatabase,
        string userEmail,
        string? personalEmail = null,
        string? triggeredBy = null,
        string? userName = null,
        string? companyType = null,
        string? industryType = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        Company = company;
        Domain = domain;
        WorkEmail = workEmail;
        PersonalEmail = personalEmail;
        FirstName = firstName;
        LastName = lastName;
        Country = country;
        EmployeeCount = employeeCount;
        PreferredDatabase = preferredDatabase;
        CompanyType = companyType;
        IndustryType = industryType;
    }

    public static TenantRegisteredEvent Create(
        string tenantId,
        string company,
        string domain,
        string workEmail,
        string firstName,
        string lastName,
        string country,
        string employeeCount,
        string preferredDatabase,
        string userEmail,
        string? personalEmail = null,
        string? triggeredBy = null,
        string? userName = null,
        string? companyType = null,
        string? industryType = null)
    {
        return new TenantRegisteredEvent(tenantId, company, domain, workEmail, firstName, lastName,
            country, employeeCount, preferredDatabase, userEmail, personalEmail, triggeredBy, userName, companyType, industryType);
    }
}

