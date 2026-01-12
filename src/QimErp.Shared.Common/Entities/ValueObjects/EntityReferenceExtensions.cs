namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Extension methods for creating EntityReference from various entity types
/// Note: Vendor and Customer extensions should be added in their respective modules
/// </summary>
public static class EntityReferenceExtensions
{
    /// <summary>
    /// Creates an EntityReference from an Employee entity
    /// </summary>
    public static EntityReference ToEntityReference(this EmployeeBase employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        return EntityReference.ForEmployee(
            employee.Id,
            employee.Code,
            $"{employee.FirstName} {employee.LastName}".Trim());
    }

    /// <summary>
    /// Creates an EntityReference from an EmployeeValueObject
    /// </summary>
    public static EntityReference ToEntityReference(this EmployeeValueObject employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        return EntityReference.ForEmployee(
            employee.Id,
            employee.Code,
            employee.Name);
    }
}
