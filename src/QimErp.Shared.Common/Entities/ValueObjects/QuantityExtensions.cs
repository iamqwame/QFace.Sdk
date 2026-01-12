namespace QimErp.Shared.Common.Entities.ValueObjects;

public static class QuantityExtensions
{
    /// <summary>
    /// Creates a Quantity from individual properties
    /// </summary>
    public static Quantity ToQuantity(
        decimal amount,
        string? unitOfMeasure = null)
    {
        return new Quantity(amount, unitOfMeasure);
    }
}
