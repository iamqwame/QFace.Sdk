namespace QimErp.Shared.Common.Entities.ValueObjects;

public class Money
{
    private const string DefaultCurrency = "GHS";
    
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = DefaultCurrency;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public decimal? BaseCurrencyAmount { get; set; }

    private Money() { }

    private Money(decimal amount, string currencyCode, decimal exchangeRate, decimal? baseCurrencyAmount)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
        ExchangeRate = exchangeRate;
        BaseCurrencyAmount = baseCurrencyAmount;
    }

    /// <summary>
    /// Creates a Money instance with the specified amount.
    /// Defaults to GHS currency if no currency is specified.
    /// </summary>
    /// <example>
    /// Money.AmountOf(1000).In("USD").WithExchangeRate(12.5m)
    /// Money.AmountOf(500) // defaults to GHS
    /// </example>
    public static Money AmountOf(decimal amount)
    {
        return new Money(amount, DefaultCurrency, 1.0m, null);
    }

    /// <summary>
    /// Sets the currency code for this Money instance.
    /// </summary>
    public Money In(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required", nameof(currencyCode));
        
        CurrencyCode = currencyCode;
        return this;
    }

    /// <summary>
    /// Sets the exchange rate for converting to base currency.
    /// </summary>
    public Money WithExchangeRate(decimal exchangeRate)
    {
        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(exchangeRate));
        
        ExchangeRate = exchangeRate;
        if (BaseCurrencyAmount == null)
        {
            BaseCurrencyAmount = Amount * ExchangeRate;
        }
        return this;
    }

    /// <summary>
    /// Sets the base currency amount explicitly.
    /// </summary>
    public Money WithBaseCurrencyAmount(decimal baseCurrencyAmount)
    {
        BaseCurrencyAmount = baseCurrencyAmount;
        return this;
    }

    public decimal GetBaseCurrencyAmount()
    {
        return BaseCurrencyAmount ?? (Amount * ExchangeRate);
    }

    public bool IsBaseCurrency(string baseCurrencyCode)
    {
        return CurrencyCode.Equals(baseCurrencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public Money ConvertToBaseCurrency(decimal exchangeRate)
    {
        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(exchangeRate));
        
        return new Money(Amount, CurrencyCode, exchangeRate, Amount * exchangeRate);
    }
}
