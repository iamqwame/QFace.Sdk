using QimErp.Shared.Common.Entities;

namespace QimErp.Shared.Common.Entities.ValueObjects;

public static class OrganizationalUnitValueObjectExtensions
{
    /// <summary>
    /// Converts an OrganizationalUnitBase to OrganizationalUnitValueObject
    /// </summary>
    public static OrganizationalUnitValueObject ToValueObject(this OrganizationalUnitBase organizationalUnit)
    {
        if (organizationalUnit == null)
            throw new ArgumentNullException(nameof(organizationalUnit));

        return new OrganizationalUnitValueObject(
            organizationalUnit.Id,
            organizationalUnit.Name,
            organizationalUnit.Code,
            organizationalUnit.Description);
    }

    /// <summary>
    /// Creates an OrganizationalUnitValueObject from individual properties
    /// </summary>
    public static OrganizationalUnitValueObject ToValueObject(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        return new OrganizationalUnitValueObject(
            id,
            name,
            code,
            description);
    }
}
