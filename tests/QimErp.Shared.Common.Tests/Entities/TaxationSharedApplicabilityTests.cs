using FluentAssertions;
using QimErp.Shared.Common.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Entities;

public class TaxationSharedApplicabilityTests
{
    private static readonly DateOnly LastCovidDay = new(2025, 12, 31);
    private static readonly DateOnly FirstDayAfterCovid = new(2026, 1, 1);

    [Fact]
    public void CovidLevyRows_KeepRateOne_CloseOn2025_12_31_AndDoNotAffectSubsequentTaxes()
    {
        var rows = TaxationShared.GetAll()
            .Where(tax => tax.Name is "COVID Levy" or "COVID Levy (Purchase)")
            .ToList();

        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(tax =>
            tax.Rate == 1m
            && tax.ValidFrom == null
            && tax.ValidTo == LastCovidDay
            && tax.AffectsSubsequentTaxes == false);
    }

    [Fact]
    public void CovidLevyRows_ApplyThrough2025_12_31_AndNotOn2026_01_01()
    {
        var rows = TaxationShared.GetAll()
            .Where(tax => tax.Name is "COVID Levy" or "COVID Levy (Purchase)");

        foreach (var tax in rows)
        {
            tax.AppliesOn(LastCovidDay).Should().BeTrue();
            tax.AppliesOn(FirstDayAfterCovid).Should().BeFalse();
        }
    }

    [Fact]
    public void NhilGetFundVatAndCstRows_StayOpen_OnBothDates()
    {
        var rows = TaxationShared.GetAll()
            .Where(tax => tax.Name is "NHIL" or "GETFund" or "VAT" or "CST" or "CST (Purchase)")
            .ToList();

        rows.Should().HaveCount(5);
        rows.Should().OnlyContain(tax => tax.ValidFrom == null && tax.ValidTo == null);
        rows.Should().OnlyContain(tax => tax.AppliesOn(LastCovidDay) && tax.AppliesOn(FirstDayAfterCovid));
    }

    [Fact]
    public void InactiveRow_IsNotApplicable()
    {
        var tax = TaxationShared.Create("Inactive levy", 1m);
        tax.IsActive = false;

        tax.AppliesOn(LastCovidDay).Should().BeFalse();
    }

    [Fact]
    public void ValidFromAfterValidTo_IsNotApplicable()
    {
        var tax = TaxationShared.Create("Inverted window", 1m)
            .WithValidFrom(new DateOnly(2026, 2, 1))
            .WithValidTo(new DateOnly(2026, 1, 1));

        tax.AppliesOn(new DateOnly(2026, 1, 15)).Should().BeFalse();
    }
}
