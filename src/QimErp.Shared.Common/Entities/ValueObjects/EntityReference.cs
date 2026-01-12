namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Entity Reference - Value object for referencing entities (Employee, Vendor, Customer, etc.) in transactions
/// Follows industry-standard polymorphic reference pattern used in major ERPs
/// </summary>
public class EntityReference
{
    public EntityReferenceType ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceCode { get; set; }
    public string? ReferenceName { get; set; }

    // Parameterless constructor for EF Core
    public EntityReference()
    {
        ReferenceType = EntityReferenceType.None;
    }

    private EntityReference(
        EntityReferenceType referenceType,
        Guid? referenceId = null,
        string? referenceCode = null,
        string? referenceName = null)
    {
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ReferenceCode = referenceCode;
        ReferenceName = referenceName;
    }

    /// <summary>
    /// Creates an EntityReference with no reference (None)
    /// </summary>
    public static EntityReference None()
    {
        return new EntityReference(EntityReferenceType.None);
    }

    /// <summary>
    /// Creates an EntityReference for an Employee
    /// </summary>
    public static EntityReference ForEmployee(Guid employeeId, string? employeeCode = null, string? employeeName = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        return new EntityReference(EntityReferenceType.Employee, employeeId, employeeCode, employeeName);
    }

    /// <summary>
    /// Creates an EntityReference for a Vendor
    /// </summary>
    public static EntityReference ForVendor(Guid vendorId, string? vendorCode = null, string? vendorName = null)
    {
        if (vendorId == Guid.Empty)
            throw new ArgumentException("Vendor ID cannot be empty", nameof(vendorId));

        return new EntityReference(EntityReferenceType.Vendor, vendorId, vendorCode, vendorName);
    }

    /// <summary>
    /// Creates an EntityReference for a Customer
    /// </summary>
    public static EntityReference ForCustomer(Guid customerId, string? customerCode = null, string? customerName = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        return new EntityReference(EntityReferenceType.Customer, customerId, customerCode, customerName);
    }

    /// <summary>
    /// Creates an EntityReference for a Project
    /// </summary>
    public static EntityReference ForProject(Guid projectId, string? projectCode = null, string? projectName = null)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        return new EntityReference(EntityReferenceType.Project, projectId, projectCode, projectName);
    }

    /// <summary>
    /// Creates an EntityReference for an external entity not in the system
    /// </summary>
    public static EntityReference ForOther(string? externalCode = null, string? externalName = null)
    {
        return new EntityReference(EntityReferenceType.Other, null, externalCode, externalName);
    }

    /// <summary>
    /// Checks if the reference has a value (not None)
    /// </summary>
    public bool HasValue => ReferenceType != EntityReferenceType.None;

    /// <summary>
    /// Checks if the reference is for an internal entity (has an ID)
    /// </summary>
    public bool IsInternalEntity => ReferenceId.HasValue && ReferenceId != Guid.Empty;
}
