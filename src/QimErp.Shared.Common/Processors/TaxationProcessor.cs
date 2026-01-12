namespace QimErp.Shared.Common.Processors;

public class TaxationProcessor(ILogger<TaxationProcessor> logger)
{
    private const int DefaultPrecision = 3;

    public TaxCalculationResult ComputeTax(
        decimal unitPrice,
        int quantity,
        decimal discount,
        List<TaxationShared> taxes)
    {
        TaxComputationContext context = new();
        var amount = unitPrice * quantity;
        return ComputeTax(amount, taxes, context);
    }
    public TaxCalculationResult ComputeTax(
        decimal unitPrice,
        int quantity,
        decimal discount,
        List<TaxationShared> taxes,
        TaxComputationContext context)
    {
        var amount = unitPrice * quantity;
        return ComputeTax(amount, taxes, context);
    }
    public TaxCalculationResult ComputeTax(
        decimal amount,
        List<TaxationShared> taxes)
    {
        TaxComputationContext context = new();
        return ComputeTax(amount, taxes, context);
    }

    public TaxCalculationResult ComputeTax(
        decimal amount,
        List<TaxationShared> taxes,
        TaxComputationContext context)
    {
        logger.LogDebug("Computing taxes. Amount: {Amount}, NumberOfTaxes: {TaxCount}, Context: {@Context}",
            amount, taxes.Count, context);

        ArgumentNullException.ThrowIfNull(taxes);
        ValidateTaxConfiguration(taxes);

        if (amount <= 0)
        {
            logger.LogDebug("Amount is zero or negative. Returning without calculation.");
            return new TaxCalculationResult(
                amount, amount,
                0,
                amount,
                Array.Empty<TaxDistributionResult>().ToList(),
                new Dictionary<string, decimal>(),
                Array.Empty<IndividualTaxRecord>().ToList());
        }

        // Filter active taxes and order by sequence
        var applicableTaxes = taxes
            .Where(t => t.IsActive)
            .OrderBy(t => t.Sequence)
            .ToList();

        if (!applicableTaxes.Any())
        {
            logger.LogDebug("No applicable taxes after filtering. Returning original amount.");
            return new TaxCalculationResult(
                amount, amount,
                0,
                amount,
                Array.Empty<TaxDistributionResult>().ToList(),
                new Dictionary<string, decimal>(),
                Array.Empty<IndividualTaxRecord>().ToList());
        }

        var baseAmount = amount;
        var adjustedBaseAmount = baseAmount; // Use this for calculations
        var totalTaxAmount = 0m;
        var allDistributions = new List<TaxDistributionResult>();
        var groupings = new Dictionary<string, decimal>();
        var individualTaxes = new List<IndividualTaxRecord>(); // New dictionary for individual taxes
        var cumulativeTax = 0m;
        var afterPrice = baseAmount; // Track AfterPrice for IncludedInPrice logic
        foreach (var tax in applicableTaxes)
        {
            // Determine base for the current tax
            if (tax.AffectsSubsequentTaxes)
            {
                adjustedBaseAmount = cumulativeTax + baseAmount;
            }
            if (tax.IncludedInPrice == AppConstant.Service.IncludedInPrice.TaxIncluded)
            {
                var (taxableBase, taxAmount) = CalculateTaxAmount(afterPrice, tax);
                afterPrice = taxableBase; // Update AfterPrice to reflect tax-included adjustments
            }

            var currentBase = tax.AffectsSubsequentTaxes ? adjustedBaseAmount : baseAmount;


            var taxResult = ProcessSingleTax(currentBase, tax, context);

            totalTaxAmount += taxResult.TaxAmount;
            allDistributions.AddRange(taxResult.Distributions);

            // Add individual tax
            individualTaxes.Add(new IndividualTaxRecord(tax.Id.ToString(), tax.Rate, tax.Name,
                taxResult.TaxAmount));

            // Group taxes by their group name and sum tax amounts
            if (!string.IsNullOrEmpty(tax.TaxGroup))
            {
                if (groupings.ContainsKey(tax.TaxGroup))
                    groupings[tax.TaxGroup] += taxResult.TaxAmount;
                else
                    groupings[tax.TaxGroup] = taxResult.TaxAmount;
            }

            cumulativeTax += taxResult.TaxAmount;
        }

        var finalAmount = RoundTaxTotal(baseAmount + totalTaxAmount, context.RoundingStrategy);

        return new TaxCalculationResult(
            amount, afterPrice,
            totalTaxAmount,
            finalAmount,
            allDistributions,
            groupings,
            individualTaxes); // Return individual taxes
    }


    private SingleTaxResult ProcessSingleTax(decimal baseAmount, TaxationShared tax, TaxComputationContext context)
    {
        var (taxableBase, taxAmount) = CalculateTaxAmount(baseAmount, tax);

        var distributions = CalculateDistributions(
            taxableBase,
            taxAmount,
            context.IsRefund ? tax.RefundDistributions : tax.InvoiceDistributions);

        return new SingleTaxResult(taxableBase, taxAmount, distributions);
    }

    private (decimal TaxableBase, decimal TaxAmount) CalculateTaxAmount(decimal amount, TaxationShared tax)
    {
        var taxableBase = amount;
        decimal taxAmount;

        switch (tax.ComputationMethod)
        {
            case AppConstant.Service.TaxComputationMethod.Fixed:
                taxAmount = tax.Rate;
                break;

            case AppConstant.Service.TaxComputationMethod.Percentage:
                if (tax.IncludedInPrice == AppConstant.Service.IncludedInPrice.TaxIncluded)
                {
                    taxableBase = amount / (1 + (tax.Rate / 100m));
                    taxAmount = amount - taxableBase; // Separate tax from the total
                }
                else
                {
                    taxAmount = amount * (tax.Rate / 100m);
                }

                break;

            case AppConstant.Service.TaxComputationMethod.PercentageOfTaxIncluded:
                // Correct formula for Percentage of Price Tax Included
                var taxRate = tax.Rate / 100m;
                taxAmount = (amount * taxRate) / (1 - taxRate);
                taxableBase = amount - taxAmount; // Base amount is total minus tax amount
                break;

            case AppConstant.Service.TaxComputationMethod.Group:
                throw new NotSupportedException("Group taxes should be handled separately");

            default:
                throw new NotSupportedException(
                    $"Unsupported tax computation method: {tax.ComputationMethod}");
        }

        // Round the taxable base and tax amount for precision
        taxableBase = Math.Round(taxableBase, DefaultPrecision, MidpointRounding.AwayFromZero);
        taxAmount = Math.Round(taxAmount, DefaultPrecision, MidpointRounding.AwayFromZero);

        return (taxableBase, taxAmount);
    }

    private IReadOnlyList<TaxDistributionResult> CalculateDistributions(
        decimal baseAmount,
        decimal taxAmount,
        IEnumerable<TaxDistributionLineShared> distributions)
    {
        logger.LogDebug("Calculating tax distributions. BaseAmount: {BaseAmount}, TaxAmount: {TaxAmount}",
            baseAmount, taxAmount);

        var results = new List<TaxDistributionResult>();

        foreach (var distribution in distributions)
        {
            var amount = distribution.BasedOn switch
            {
                AppConstant.Service.Core.DistributionBaseOnTheBase =>
                    baseAmount * (distribution.Percentage / 100m),
                AppConstant.Service.Core.DistributionBaseOnThePercentageOfTheBase =>
                    taxAmount * (distribution.Percentage / 100m),
                _ => throw new InvalidOperationException($"Unknown BasedOn value: {distribution.BasedOn}")
            };

            results.Add(new TaxDistributionResult(
                distribution.Account,
                distribution.TaxGrid,
                Math.Round(amount, 2)));
        }

        return results;
    }

    private void ValidateTaxConfiguration(List<TaxationShared> taxes)
    {
        foreach (var tax in taxes)
        {
            // Validate distributions sum to 100%
            var invoiceTotal = tax.InvoiceDistributions
                .Where(x => x.BasedOn != AppConstant.Service.Core.DistributionBaseOnTheBase)
                .Sum(d => d.Percentage);
            var refundTotal = tax.RefundDistributions
                .Where(x => x.BasedOn != AppConstant.Service.Core.DistributionBaseOnTheBase)
                .Sum(d => d.Percentage);

            if (Math.Abs(invoiceTotal - 100m) > 0.01m || Math.Abs(refundTotal - 100m) > 0.01m)
            {
                throw new InvalidOperationException(
                    $"Tax {tax.Name} distributions must sum to 100%. Found: Invoice={invoiceTotal}, Refund={refundTotal}");
            }

            // Validate that all distributions have valid "BasedOn" values
            foreach (var distribution in tax.InvoiceDistributions.Concat(tax.RefundDistributions))
            {
                if (distribution.BasedOn != AppConstant.Service.Core.DistributionBaseOnTheBase &&
                    distribution.BasedOn != AppConstant.Service.Core.DistributionBaseOnThePercentageOfTheBase)
                {
                    throw new InvalidOperationException($"Invalid BasedOn value in tax {tax.Name}");
                }
            }
        }
    }

    private decimal RoundTaxTotal(decimal amount, TaxRoundingStrategy strategy)
    {
        return strategy switch
        {
            TaxRoundingStrategy.PerLine => amount,
            TaxRoundingStrategy.Globally => Math.Round(amount, DefaultPrecision, MidpointRounding.AwayFromZero),
            _ => throw new NotSupportedException($"Unsupported rounding strategy: {strategy}")
        };
    }
}

public enum TaxRoundingStrategy
{
    PerLine,
    Globally
}

public record TaxComputationContext(
    bool IsRefund = false,
    TaxRoundingStrategy RoundingStrategy = TaxRoundingStrategy.PerLine);


public class TaxDistributionRequest
{
    public decimal? Percentage { get; set; }
    public string BasedOn { get; set; }
    public AccountProperty Account { get; set; }
    public string TaxGrid { get; set; }
}
public class SingleTaxResult
{
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public IReadOnlyList<TaxDistributionResult> Distributions { get; set; }

    public SingleTaxResult(decimal baseAmount, decimal taxAmount, IReadOnlyList<TaxDistributionResult> distributions)
    {
        BaseAmount = baseAmount;
        TaxAmount = taxAmount;
        Distributions = distributions;
    }
}

public class TaxCalculationResult
{
    public decimal BaseAmount { get; set; }
    public decimal AfterPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<TaxDistributionResult> Distributions { get; set; }
    public Dictionary<string, decimal> Grouping { get; set; }
    public List<IndividualTaxRecord> IndividualTaxes { get; set; }

    public TaxCalculationResult(
        decimal baseAmount,
        decimal afterPrice,
        decimal taxAmount,
        decimal totalAmount,
        List<TaxDistributionResult> distributions,
        Dictionary<string, decimal> grouping,
        List<IndividualTaxRecord> individualTaxes)
    {
        BaseAmount = baseAmount;
        AfterPrice = afterPrice;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Distributions = distributions;
        Grouping = grouping;
        IndividualTaxes = individualTaxes;
    }
}

public class IndividualTaxRecord
{
    public string Id { get; set; }
    public decimal Rate { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }

    public IndividualTaxRecord(string id, decimal rate, string name, decimal amount)
    {
        Id = id;
        Rate = rate;
        Name = name;
        Amount = amount;
    }
}

public class TaxDistributionResult
{
    public AccountProperty Account { get; set; }
    public string TaxGrid { get; set; }
    public decimal Amount { get; set; }

    public TaxDistributionResult(AccountProperty account, string taxGrid, decimal amount)
    {
        Account = account;
        TaxGrid = taxGrid;
        Amount = amount;
    }
}

