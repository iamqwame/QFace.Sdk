namespace QimErp.Shared.Common.Entities;

public class TaxationShared : GuidAuditableEntity
{
    // Private backing fields for distributions
    private readonly List<TaxDistributionLineShared> _invoiceDistributions = [];
    private readonly List<TaxDistributionLineShared> _refundDistributions = [];
    public string Name { get; set; }
    public decimal Rate { get; set; }
    public string ComputationMethod { get; set; }
    public bool IsActive { get; set; } = true; // Defaults to true
    public string Scope { get; set; }
    public string Type { get; set; }
    public string LabelOnInvoices { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TaxGroup { get; set; }
    public string Country { get; set; }
    public string LegalNotes { get; set; } = string.Empty;
    public string IncludedInPrice { get; set; }
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
    public TaxationShared WithComputationMethod(string method = AppConstant.Service.TaxComputationMethod.Percentage)
    {
        ComputationMethod = method;
        return this;
    }

    public TaxationShared WithTaxGroup(string group = "Tax")
    {
        TaxGroup = group;
        return this;
    }


    public TaxationShared WithScope(string scope = AppConstant.Service.TaxScope.Default)
    {
        Scope = scope;
        return this;
    }

    public TaxationShared WithIncludedInPrice(string includedInPrice = AppConstant.Service.IncludedInPrice.Default)
    {
        IncludedInPrice = includedInPrice;
        return this;
    }

    public TaxationShared WithType(string type = AppConstant.Service.TaxType.Default)
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
        if (baseOn == AppConstant.Service.Core.DistributionBaseOnTheBase)
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
        if (baseOn == AppConstant.Service.Core.DistributionBaseOnTheBase)
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
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup()
                .WithSequence(5)
                .WithDescription("National Health Insurance Levy")
                .AddDistribution(),

            Create("GETFUND", 2.5m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup()
                .WithSequence(5)
                .WithDescription("Ghana Education Trust Fund")
                .AddDistribution(),

            Create("NHIL", 2.5m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup()
                .WithSequence(5)
                .WithDescription("National Health Insurance Levy")
                .AddDistribution(),

            Create("COVID-19", 1m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup()
                .WithSequence(5)
                .WithDescription("COVID-19 Health Recovery Levy")
                .AddDistribution(),

            Create("Standard VAT Rate", 12.5m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup()
                .WithSequence(5)
                .WithDescription("General taxable goods and services.")
                .AddDistribution(),

            Create("Standard VAT Rate", 12.5m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup("VAT")
                .WithSequence(5)
                .WithDescription("General taxable goods and services.")
                .AddDistribution(),

            Create("VAT Flat Rate Scheme (VFRS)", 3m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup("VAT")
                .WithLabelOnInvoices("3% VAT")
                .WithSequence(5)
                .WithDescription("Small-scale retailers and wholesalers")
                .AddDistribution(),

            Create("VAT Exempt", 0m)
                .WithComputationMethod()
                .WithType(AppConstant.Service.TaxType.Sales)
                .WithTaxGroup("VAT")
                .WithLabelOnInvoices("0% VAT")
                .WithSequence(5)
                .WithDescription("Healthcare, education, financial services, and specified agricultural goods.")
                .AddDistribution()
        ];
    }

    private TaxationShared AddDistribution(string accountId = "451000", string accountName = "VAT Payable")
    {
        AddInvoiceDistribution(AppConstant.Service.Core.DistributionBaseOnTheBase, null, "FIXED_01", accountId,
            accountName);
        AddInvoiceDistribution(AppConstant.Service.Core.DistributionBaseOnThePercentageOfTheBase, 100, "FIXED_01",
            accountId,
            accountName);
        AddRefundDistribution(AppConstant.Service.Core.DistributionBaseOnTheBase, null, "FIXED_01", "451000",
            accountName);
        AddRefundDistribution(AppConstant.Service.Core.DistributionBaseOnThePercentageOfTheBase, 100, "FIXED_01",
            accountId,
            accountName);
        return this;
    }
}

public class TaxDistributionLineShared
{
    public Guid Id { get; private set; }
    public decimal Percentage { get; private set; } // Percentage of allocation (e.g., 100% or 50%)
    public string BasedOn { get; private set; } // "Base" or "% of Tax"
    public AccountProperty Account { get; private set; } // Financial account for tax (e.g., "451000 VAT Payable")
    public string TaxGrid { get; private set; } // Tax grid reference for reporting

    // Fluent method for creating a TaxDistribution
    public static TaxDistributionLineShared CreatePercentageOfBase(decimal percentage, AccountProperty account,
        string taxGrid)
    {
        return new TaxDistributionLineShared
        {
            Percentage = percentage,
            BasedOn = AppConstant.Service.Core.DistributionBaseOnThePercentageOfTheBase,
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
            BasedOn = AppConstant.Service.Core.DistributionBaseOnTheBase,
            Account = account,
            TaxGrid = taxGrid,
            Id = Guid.CreateVersion7()
        };
    }
}