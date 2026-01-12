namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing quarterly budget amounts (Q1, Q2, Q3, Q4).
/// Provides fluent API for setting quarterly amounts.
/// </summary>
public class QuarterlyAmounts
{
    public Money? Q1 { get; set; }
    public Money? Q2 { get; set; }
    public Money? Q3 { get; set; }
    public Money? Q4 { get; set; }

    // Constructors
    public QuarterlyAmounts() { }

    public QuarterlyAmounts(Money? q1, Money? q2, Money? q3, Money? q4)
    {
        Q1 = q1;
        Q2 = q2;
        Q3 = q3;
        Q4 = q4;
    }

    /// <summary>
    /// Creates a new QuarterlyAmounts instance.
    /// </summary>
    public static QuarterlyAmounts Create() => new();

    /// <summary>
    /// Sets the Q1 amount.
    /// </summary>
    public QuarterlyAmounts InQ1(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("Q1 amount cannot be negative", nameof(amount));
        
        Q1 = amount;
        return this;
    }

    /// <summary>
    /// Sets the Q2 amount.
    /// </summary>
    public QuarterlyAmounts InQ2(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("Q2 amount cannot be negative", nameof(amount));
        
        Q2 = amount;
        return this;
    }

    /// <summary>
    /// Sets the Q3 amount.
    /// </summary>
    public QuarterlyAmounts InQ3(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("Q3 amount cannot be negative", nameof(amount));
        
        Q3 = amount;
        return this;
    }

    /// <summary>
    /// Sets the Q4 amount.
    /// </summary>
    public QuarterlyAmounts InQ4(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("Q4 amount cannot be negative", nameof(amount));
        
        Q4 = amount;
        return this;
    }

    /// <summary>
    /// Gets the total of all quarterly amounts.
    /// </summary>
    public Money? GetTotal(string currencyCode)
    {
        var total = (Q1?.Amount ?? 0) + (Q2?.Amount ?? 0) + (Q3?.Amount ?? 0) + (Q4?.Amount ?? 0);
        if (total == 0 && Q1 == null && Q2 == null && Q3 == null && Q4 == null)
            return null;
        
        return Money.AmountOf(total).In(currencyCode);
    }
}
