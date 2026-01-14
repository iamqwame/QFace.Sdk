namespace QimErp.Shared.Common.Entities.ValueObjects;

public static class CostCenterValueObjectExtensions
{
    /// <summary>
    /// Converts a CostCenterBase to CostCenterValueObject
    /// </summary>
    public static CostCenterValueObject ToValueObject(this CostCenterBase costCenter)
    {
        if (costCenter == null)
            throw new ArgumentNullException(nameof(costCenter));

        return new CostCenterValueObject(
            costCenter.Id,
            costCenter.Code,
            costCenter.Name,
            costCenter.Description);
    }

    /// <summary>
    /// Creates a CostCenterValueObject from individual properties
    /// </summary>
    public static CostCenterValueObject ToValueObject(
        Guid id,
        string code,
        string name,
        string? description = null)
    {
        return new CostCenterValueObject(
            id,
            code,
            name,
            description);
    }
}
