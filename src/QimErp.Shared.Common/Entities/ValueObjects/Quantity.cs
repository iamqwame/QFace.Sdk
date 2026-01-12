namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a quantity with unit of measure.
/// Used for inventory, materials, and other measurable items.
/// </summary>
public class Quantity
{
    public decimal Amount { get; set; }
    public string? UnitOfMeasure { get; set; }

    // Constructors
    public Quantity() { }

    public Quantity(decimal amount, string? unitOfMeasure = null)
    {
        if (amount < 0)
            throw new ArgumentException("Quantity amount cannot be negative", nameof(amount));

        Amount = amount;
        UnitOfMeasure = unitOfMeasure;
    }

    /// <summary>
    /// Creates a Quantity instance with the specified amount.
    /// </summary>
    /// <example>
    /// Quantity.Of(100).In("kg")
    /// Quantity.Of(50) // no unit
    /// </example>
    public static Quantity Of(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Quantity amount cannot be negative", nameof(amount));

        return new Quantity(amount, null);
    }

    /// <summary>
    /// Sets the unit of measure for this Quantity instance.
    /// </summary>
    public Quantity In(string? unitOfMeasure)
    {
        UnitOfMeasure = unitOfMeasure;
        return this;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Quantity other)
            return false;

        return Amount == other.Amount && 
               string.Equals(UnitOfMeasure, other.UnitOfMeasure, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, UnitOfMeasure?.ToUpperInvariant());
    }

    public override string ToString()
    {
        return UnitOfMeasure != null 
            ? $"{Amount} {UnitOfMeasure}" 
            : Amount.ToString();
    }
}
