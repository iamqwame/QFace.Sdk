using QimErp.Shared.Common.Processors;

namespace QimErp.Shared.Common.Entities;

public class TaxationShared : GuidAuditableEntity
{
    // Private backing fields for distributions
    private readonly List<TaxDistributionLineShared> _invoiceDistributions = [];
    private readonly List<TaxDistributionLineShared> _refundDistributions = [];
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string ComputationMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true; // Defaults to true
    public string Scope { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string LabelOnInvoices { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TaxGroup { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string LegalNotes { get; set; } = string.Empty;
    public string IncludedInPrice { get; set; } = string.Empty;
    public bool AffectsSubsequentTaxes { get; set; } // Defaults to false

    // Public read-only access to distributions
    public IReadOnlyList<TaxDistributionLineShared> InvoiceDistributions => _invoiceDistributions.AsReadOnly();
    public IReadOnlyList<TaxDistributionLineShared> RefundDistributions => _refundDistributions.AsReadOnly();
    public int Sequence { get; set; }

    public static TaxationShared Create(string name, decimal rate)
    {
        return new TaxationShared
        {
            Name = name,
            Rate = rate,
            IsActive = true,
            LabelOnInvoices = name,
            Country = "Ghana"
        }.WithComputationMethod()
            .WithType()
            .WithIncludedInPrice()
            .WithScope()
            .WithTaxGroup();
    }

    // Fluent methods for property assignment
    public TaxationShared WithComputationMethod(string method = TaxationConstants.TaxComputationMethod.Percentage)
    {
        ComputationMethod = method;
        return this;
    }

    public TaxationShared WithTaxGroup(string group = "Tax")
    {
        TaxGroup = group;
        return this;
    }


    public TaxationShared WithScope(string scope = TaxationConstants.TaxScope.Default)
    {
        Scope = scope;
        return this;
    }

    public TaxationShared WithIncludedInPrice(string includedInPrice = TaxationConstants.IncludedInPrice.Default)
    {
        IncludedInPrice = includedInPrice;
        return this;
    }

    public TaxationShared WithType(string type = TaxationConstants.TaxType.Default)
    {
        Type = type;
        return this;
    }

    public TaxationShared WithLabelOnInvoices(string label)
    {
        LabelOnInvoices = label;
        return this;
    }

    public TaxationShared WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public TaxationShared WithName(string name)
    {
        Name = name;
        return this;
    }

    public TaxationShared WithRate(decimal rate)
    {
        Rate = rate;
        return this;
    }

    public TaxationShared WithCountry(string country)
    {
        Country = country;
        return this;
    }

    public TaxationShared WithLegalNotes(string notes)
    {
        LegalNotes = notes;
        return this;
    }


    public TaxationShared WithAffectsSubsequentTaxes(bool affects)
    {
        AffectsSubsequentTaxes = affects;
        return this;
    }

    public TaxationShared WithSequence(int sequenceNo)
    {
        Sequence = sequenceNo;
        return this;
    }

    public TaxationShared AddInvoiceDistribution(string baseOn, decimal? percentage, string taxGrid, string? accountId,
        string? accountName, string? accountCode = "")
    {
        if (baseOn == TaxationConstants.Core.DistributionBaseOnTheBase)
        {
            _invoiceDistributions.Add(TaxDistributionLineShared.CreateBase(
                AccountProperty.Create(accountId, accountName, accountCode), taxGrid));
        }
        else
        {
            _invoiceDistributions.Add(TaxDistributionLineShared.CreatePercentageOfBase(
                percentage ?? 0, AccountProperty.Create(accountId, accountName, accountCode), taxGrid));
        }

        return this;
    }

    public TaxationShared AddRefundDistribution(string baseOn, decimal? percentage, string taxGrid, string? accountId,
        string? accountName, string? accountCode = "")
    {
        if (baseOn == TaxationConstants.Core.DistributionBaseOnTheBase)
        {
            _refundDistributions.Add(TaxDistributionLineShared.CreateBase(
                AccountProperty.Create(accountId, accountName, accountCode), taxGrid));
        }
        else
        {
            _refundDistributions.Add(TaxDistributionLineShared.CreatePercentageOfBase(
                percentage ?? 0, AccountProperty.Create(accountId, accountName, accountCode), taxGrid));
        }

        return this;
    }

    public static List<TaxationShared> GetAll()
    {
        return
        [
            Create("NHIL", 2.5m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Sales)
                .WithTaxGroup("NHIL")
                .WithSequence(1)
                .WithDescription("National Health Insurance Levy (VAT Act 2025 / Act 1151)")
                .WithLegalNotes("Creditable input tax from Jan 2026")
                .AddDistribution("20302", "NHIL Output Payable"),

            Create("GETFund", 2.5m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Sales)
                .WithTaxGroup("GETFund")
                .WithSequence(2)
                .WithDescription("Ghana Education Trust Fund Levy (VAT Act 2025 / Act 1151)")
                .WithLegalNotes("Creditable input tax from Jan 2026")
                .AddDistribution("20303", "GETFund Output Payable"),

            Create("VAT", 15m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Sales)
                .WithTaxGroup("VAT")
                .WithSequence(3)
                .WithDescription("Standard VAT rate per VAT Act 2025 (Act 1151), effective Jan 2026")
                .AddDistribution("20301", "VAT Output Payable"),

            Create("VAT Exempt", 0m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Sales)
                .WithTaxGroup("VAT")
                .WithLabelOnInvoices("0% VAT")
                .WithSequence(4)
                .WithDescription("Healthcare, education, financial services, and specified agricultural goods.")
                .AddDistribution(),

            Create("NHIL (Purchase)", 2.5m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Purchases)
                .WithTaxGroup("NHIL")
                .WithSequence(5)
                .WithDescription("Input NHIL — creditable per Act 1151")
                .AddDistribution("50700", "Purchase Tax"),

            Create("GETFund (Purchase)", 2.5m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Purchases)
                .WithTaxGroup("GETFund")
                .WithSequence(6)
                .WithDescription("Input GETFund — creditable per Act 1151")
                .AddDistribution("50700", "Purchase Tax"),

            Create("VAT (Purchase)", 15m)
                .WithComputationMethod()
                .WithType(TaxationConstants.TaxType.Purchases)
                .WithTaxGroup("VAT")
                .WithSequence(7)
                .WithDescription("Input VAT at 15% per Act 1151")
                .AddDistribution("50700", "Purchase Tax")
        ];
    }

    private TaxationShared AddDistribution(string accountCode = "20301", string accountName = "VAT Output Payable")
    {
        AddInvoiceDistribution(TaxationConstants.Core.DistributionBaseOnTheBase, null, "FIXED_01", accountCode,
            accountName);
        AddInvoiceDistribution(TaxationConstants.Core.DistributionBaseOnThePercentageOfTheBase, 100, "FIXED_01",
            accountCode,
            accountName);
        AddRefundDistribution(TaxationConstants.Core.DistributionBaseOnTheBase, null, "FIXED_01", accountCode,
            accountName);
        AddRefundDistribution(TaxationConstants.Core.DistributionBaseOnThePercentageOfTheBase, 100, "FIXED_01",
            accountCode,
            accountName);
        return this;
    }
}

public class TaxDistributionLineShared
{
    public Guid Id { get; private set; }
    public decimal Percentage { get; private set; } // Percentage of allocation (e.g., 100% or 50%)
    public required string BasedOn { get; init; } // "Base" or "% of Tax"
    public required AccountProperty Account { get; init; } // Financial account for tax (e.g., "451000 VAT Payable")
    public required string TaxGrid { get; init; } // Tax grid reference for reporting

    // Fluent method for creating a TaxDistribution
    public static TaxDistributionLineShared CreatePercentageOfBase(decimal percentage, AccountProperty account,
        string taxGrid)
    {
        return new TaxDistributionLineShared
        {
            Percentage = percentage,
            BasedOn = TaxationConstants.Core.DistributionBaseOnThePercentageOfTheBase,
            Account = account,
            TaxGrid = taxGrid,
            Id = Guid.CreateVersion7()
        };
    }

    public static TaxDistributionLineShared CreateBase(AccountProperty account, string taxGrid)
    {
        return new TaxDistributionLineShared
        {
            Percentage = 100,
            BasedOn = TaxationConstants.Core.DistributionBaseOnTheBase,
            Account = account,
            TaxGrid = taxGrid,
            Id = Guid.CreateVersion7()
        };
    }
}