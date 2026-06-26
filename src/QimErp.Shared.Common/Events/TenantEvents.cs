namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a new tenant is registered
/// This event is published by Auth module after successful tenant registration
/// and consumed by HR modules to perform initial setup/seeding
/// </summary>
public class TenantRegisteredEvent : DomainEvent
{
    public string Company { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string WorkEmail { get; set; } = string.Empty;
    public string? PersonalEmail { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string EmployeeCount { get; set; } = string.Empty;
    public string PreferredDatabase { get; set; } = string.Empty;
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

