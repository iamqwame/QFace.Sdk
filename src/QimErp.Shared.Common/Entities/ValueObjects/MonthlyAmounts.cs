namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing monthly budget amounts (January through December).
/// Provides fluent API for setting monthly amounts.
/// </summary>
public class MonthlyAmounts
{
    public Money? January { get; set; }
    public Money? February { get; set; }
    public Money? March { get; set; }
    public Money? April { get; set; }
    public Money? May { get; set; }
    public Money? June { get; set; }
    public Money? July { get; set; }
    public Money? August { get; set; }
    public Money? September { get; set; }
    public Money? October { get; set; }
    public Money? November { get; set; }
    public Money? December { get; set; }

    // Constructors
    public MonthlyAmounts() { }

    public MonthlyAmounts(
        Money? january, Money? february, Money? march, Money? april,
        Money? may, Money? june, Money? july, Money? august,
        Money? september, Money? october, Money? november, Money? december)
    {
        January = january;
        February = february;
        March = march;
        April = april;
        May = may;
        June = june;
        July = july;
        August = august;
        September = september;
        October = october;
        November = november;
        December = december;
    }

    /// <summary>
    /// Creates a new MonthlyAmounts instance.
    /// </summary>
    public static MonthlyAmounts Create() => new();

    /// <summary>
    /// Sets the January amount.
    /// </summary>
    public MonthlyAmounts InJanuary(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("January amount cannot be negative", nameof(amount));
        
        January = amount;
        return this;
    }

    /// <summary>
    /// Sets the February amount.
    /// </summary>
    public MonthlyAmounts InFebruary(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("February amount cannot be negative", nameof(amount));
        
        February = amount;
        return this;
    }

    /// <summary>
    /// Sets the March amount.
    /// </summary>
    public MonthlyAmounts InMarch(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("March amount cannot be negative", nameof(amount));
        
        March = amount;
        return this;
    }

    /// <summary>
    /// Sets the April amount.
    /// </summary>
    public MonthlyAmounts InApril(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("April amount cannot be negative", nameof(amount));
        
        April = amount;
        return this;
    }

    /// <summary>
    /// Sets the May amount.
    /// </summary>
    public MonthlyAmounts InMay(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("May amount cannot be negative", nameof(amount));
        
        May = amount;
        return this;
    }

    /// <summary>
    /// Sets the June amount.
    /// </summary>
    public MonthlyAmounts InJune(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("June amount cannot be negative", nameof(amount));
        
        June = amount;
        return this;
    }

    /// <summary>
    /// Sets the July amount.
    /// </summary>
    public MonthlyAmounts InJuly(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("July amount cannot be negative", nameof(amount));
        
        July = amount;
        return this;
    }

    /// <summary>
    /// Sets the August amount.
    /// </summary>
    public MonthlyAmounts InAugust(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("August amount cannot be negative", nameof(amount));
        
        August = amount;
        return this;
    }

    /// <summary>
    /// Sets the September amount.
    /// </summary>
    public MonthlyAmounts InSeptember(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("September amount cannot be negative", nameof(amount));
        
        September = amount;
        return this;
    }

    /// <summary>
    /// Sets the October amount.
    /// </summary>
    public MonthlyAmounts InOctober(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("October amount cannot be negative", nameof(amount));
        
        October = amount;
        return this;
    }

    /// <summary>
    /// Sets the November amount.
    /// </summary>
    public MonthlyAmounts InNovember(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("November amount cannot be negative", nameof(amount));
        
        November = amount;
        return this;
    }

    /// <summary>
    /// Sets the December amount.
    /// </summary>
    public MonthlyAmounts InDecember(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount < 0)
            throw new ArgumentException("December amount cannot be negative", nameof(amount));
        
        December = amount;
        return this;
    }

    /// <summary>
    /// Gets the total of all monthly amounts.
    /// </summary>
    public Money? GetTotal(string currencyCode)
    {
        var total = (January?.Amount ?? 0) + (February?.Amount ?? 0) + (March?.Amount ?? 0) +
                   (April?.Amount ?? 0) + (May?.Amount ?? 0) + (June?.Amount ?? 0) +
                   (July?.Amount ?? 0) + (August?.Amount ?? 0) + (September?.Amount ?? 0) +
                   (October?.Amount ?? 0) + (November?.Amount ?? 0) + (December?.Amount ?? 0);
        
        if (total == 0 && January == null && February == null && March == null &&
            April == null && May == null && June == null && July == null &&
            August == null && September == null && October == null &&
            November == null && December == null)
            return null;
        
        return Money.AmountOf(total).In(currencyCode);
    }
}
