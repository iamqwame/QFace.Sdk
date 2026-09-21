using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Processors;
using Xunit;

namespace QimErp.Shared.Common.Tests.Processors;

public sealed class TaxationProcessorDiscountTests
{
    private readonly TaxationProcessor _processor = new(NullLogger<TaxationProcessor>.Instance);

    private static TaxationShared Tax(string name, decimal rate, int sequence) =>
        TaxationShared.Create(name, rate).WithSequence(sequence).WithTaxGroup(name)
            .AddInvoiceDistribution(TaxationConstants.Core.DistributionBaseOnThePercentageOfTheBase, 100m, "GRID", "acc-1", name)
            .AddRefundDistribution(TaxationConstants.Core.DistributionBaseOnThePercentageOfTheBase, 100m, "GRID", "acc-1", name);

    [Fact]
    public void ComputeTax_AppliesDiscountBeforeTax()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(100m, 2, 10m, taxes);

        result.BaseAmount.Should().Be(180m);
        result.TaxAmount.Should().Be(27.000m);
        result.TaxAmount.Should().NotBe(30m);
        result.TotalAmount.Should().Be(207.000m);
        result.AfterPrice.Should().Be(180m);
        result.IndividualTaxes.Should().ContainSingle();
        result.IndividualTaxes[0].Amount.Should().Be(27.000m);
        result.Distributions.Should().ContainSingle();
        result.Distributions[0].Amount.Should().Be(27.00m);
        result.Grouping["VAT"].Should().Be(27.000m);
    }

    [Fact]
    public void ComputeTax_WithZeroDiscount_IsUnchanged()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(100m, 2, 0m, taxes);

        result.BaseAmount.Should().Be(200m);
        result.TaxAmount.Should().Be(30.000m);
        result.TotalAmount.Should().Be(230.000m);
    }

    [Fact]
    public void ComputeTax_WithFullDiscount_ReturnsZeroTax()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(100m, 2, 100m, taxes);

        result.BaseAmount.Should().Be(0m);
        result.AfterPrice.Should().Be(0m);
        result.TaxAmount.Should().Be(0m);
        result.TotalAmount.Should().Be(0m);
        result.Distributions.Should().BeEmpty();
        result.Grouping.Should().BeEmpty();
        result.IndividualTaxes.Should().BeEmpty();
    }

    [Fact]
    public void ComputeTax_WithDiscountAbove100_ReturnsZeroTaxAndDoesNotThrow()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var act = () => _processor.ComputeTax(100m, 2, 150m, taxes);

        act.Should().NotThrow();

        var result = _processor.ComputeTax(100m, 2, 150m, taxes);

        result.BaseAmount.Should().Be(-100m);
        result.TaxAmount.Should().Be(0m);
        result.TotalAmount.Should().Be(-100m);
        result.IndividualTaxes.Should().BeEmpty();
    }

    [Fact]
    public void ComputeTax_WithNegativeDiscount_TaxesTheInflatedBase()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(100m, 2, -10m, taxes);

        result.BaseAmount.Should().Be(220m);
        result.TaxAmount.Should().Be(33.000m);
        result.TotalAmount.Should().Be(253.000m);
    }

    [Fact]
    public void ComputeTax_DoesNotPreRoundTheDiscountedBase()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(33.33m, 11, 17.5m, taxes);

        result.BaseAmount.Should().Be(302.46975m);
        result.TaxAmount.Should().Be(45.370m);
        result.TaxAmount.Should().NotBe(45.371m);
        result.TotalAmount.Should().Be(347.83975m);
    }

    [Theory]
    [InlineData(100, 2, 10)]
    [InlineData(33.33, 11, 17.5)]
    [InlineData(33.33, 3, 7.5)]
    [InlineData(12.345, 4, 7.5)]
    [InlineData(19.99, 7, 33.33)]
    public void ComputeTax_BaseAmountEqualsEntityLineTotalBeforeTax(decimal unitPrice, int quantity, decimal discount)
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(unitPrice, quantity, discount, taxes);

        var lineAmount = unitPrice * quantity;
        result.BaseAmount.Should().Be(lineAmount - (discount / 100m * lineAmount));
    }

    [Fact]
    public void ComputeTax_MultipleSequentialTaxes_AllUseDiscountedBase()
    {
        var taxes = new List<TaxationShared>
        {
            Tax("NHIL", 2.5m, 1),
            Tax("GETFund", 2.5m, 2),
            Tax("VAT", 15m, 3)
        };

        var result = _processor.ComputeTax(100m, 1, 10m, taxes);

        result.BaseAmount.Should().Be(90m);
        result.IndividualTaxes.Should().HaveCount(3);
        result.IndividualTaxes[0].Amount.Should().Be(2.250m);
        result.IndividualTaxes[1].Amount.Should().Be(2.250m);
        result.IndividualTaxes[2].Amount.Should().Be(13.500m);
        result.TaxAmount.Should().Be(18.000m);
        result.TotalAmount.Should().Be(108.000m);
        result.Grouping["NHIL"].Should().Be(2.250m);
        result.Grouping["GETFund"].Should().Be(2.250m);
        result.Grouping["VAT"].Should().Be(13.500m);
    }

    [Fact]
    public void ComputeTax_FiveArgOverload_MatchesFourArgOverload()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(100m, 2, 10m, taxes, new TaxComputationContext());

        result.BaseAmount.Should().Be(180m);
        result.TaxAmount.Should().Be(27.000m);
        result.TotalAmount.Should().Be(207.000m);
    }

    [Fact]
    public void ComputeTax_FiveArgOverload_RefundContextUsesRefundDistributions()
    {
        var tax = Tax("VAT", 15m, 1);
        var taxes = new List<TaxationShared> { tax };

        var result = _processor.ComputeTax(100m, 2, 10m, taxes, new TaxComputationContext(IsRefund: true));

        result.TaxAmount.Should().Be(27.000m);
        result.Distributions.Should().ContainSingle();
        result.Distributions[0].TaxGrid.Should().Be(tax.RefundDistributions[0].TaxGrid);
        result.Distributions[0].Account.Should().BeEquivalentTo(tax.RefundDistributions[0].Account);
        result.Distributions[0].Amount.Should().Be(27.00m);
    }

    [Fact]
    public void ComputeTax_AmountOverload_IsUnaffected()
    {
        var taxes = new List<TaxationShared> { Tax("VAT", 15m, 1) };

        var result = _processor.ComputeTax(180m, taxes);

        result.BaseAmount.Should().Be(180m);
        result.TaxAmount.Should().Be(27.000m);
        result.TotalAmount.Should().Be(207.000m);
    }
}
