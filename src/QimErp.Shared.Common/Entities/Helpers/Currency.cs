namespace QimErp.Shared.Common.Entities.Helpers;

public class Currency
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string Minor { get; set; } = string.Empty;

    public decimal SellingRate { get; set; } = 1;
    public decimal BuyRate { get; set; } = 1;

    public static Currency Get(string code)
    {
        Currency? currency = GetAll()
            .FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.CurrentCultureIgnoreCase));
        if (currency == null)
        {
            throw new DomainException("CurrencyNotFound", $"Currency with code '{code}' was not found.");
        }

        return currency;
    }

    public Currency WithRateOf(decimal sellingRate, decimal buyRate)
    {
        SellingRate = sellingRate;
        BuyRate = buyRate;
        return this;
    }

    private static IEnumerable<Currency> GetAll()
    {
        return new List<Currency>
        {
            new()
            {
                Code = "GHS",
                Symbol = "GHS",
                Name = "Ghana Cedi",
                Major = "Ghana Cedis",
                Minor = "Pesewas"
            },
            new()
            {
                Code = "USD",
                Symbol = "$",
                Name = "US Dollar",
                Major = "US Dollar",
                Minor = "Cent"
            }
        };
    }
}