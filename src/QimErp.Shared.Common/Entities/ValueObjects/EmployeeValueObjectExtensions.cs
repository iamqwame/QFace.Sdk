namespace QimErp.Shared.Common.Entities.ValueObjects;

public static class EmployeeValueObjectExtensions
{
    /// <summary>
    /// Converts an EmployeeBase to EmployeeValueObject. Denormalised job-info
    /// fields are populated from the base's flat Current* slots (rank is not
    /// tracked on EmployeeBase, so RankCode/Rank are left null here).
    /// </summary>
    public static EmployeeValueObject ToValueObject(this Entities.EmployeeBase employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        return new EmployeeValueObject(
            employee.Id,
            employee.Code,
            employee.FullName,
            employee.Email ?? string.Empty,
            employee.ProfilePicture ?? string.Empty)
        {
            JobTitle               = employee.CurrentJobTitleName,
            JobTitleCode           = employee.CurrentJobTitleCode,
            OrganizationalUnit     = employee.CurrentOrganizationalUnitName,
            OrganizationalUnitCode = employee.CurrentOrganizationalUnitCode,
            Station                = employee.CurrentStationName,
            StationCode            = employee.CurrentStationCode,
        };
    }

    /// <summary>
    /// Creates an EmployeeValueObject from individual properties
    /// </summary>
    public static EmployeeValueObject ToValueObject(
        Guid id,
        string code,
        string name,
        string? email = null,
        string? picture = null)
    {
        return new EmployeeValueObject(
            id,
            code,
            name,
            email ?? string.Empty,
            picture ?? string.Empty);
    }
}

