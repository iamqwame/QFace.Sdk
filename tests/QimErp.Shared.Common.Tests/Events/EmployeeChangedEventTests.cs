using FluentAssertions;
using QimErp.Shared.Common.Events;
using Xunit;

namespace QimErp.Shared.Common.Tests.Events;

public sealed class EmployeeChangedEventTests
{
    [Fact(DisplayName = "WithCompanyId and WithVisibilityAcrossCompanies round-trip")]
    public void CompanyId_and_visibility_round_trip()
    {
        var @event = EmployeeChangedEvent
            .Create("ama@qimerp.com", "Ama", "Owusu", "tenant-1", "system@qimerp.com")
            .WithCompanyId("company-a")
            .WithVisibilityAcrossCompanies(true);

        @event.CompanyId.Should().Be("company-a");
        @event.IsVisibleAcrossCompanies.Should().BeTrue();
    }

    [Fact(DisplayName = "WithCompanyId trims whitespace")]
    public void WithCompanyId_trims_whitespace()
    {
        var @event = new EmployeeChangedEvent().WithCompanyId("  company-b  ");

        @event.CompanyId.Should().Be("company-b");
    }

    [Fact(DisplayName = "WithCompanyId(null) yields empty string")]
    public void WithCompanyId_null_yields_empty()
    {
        var @event = new EmployeeChangedEvent().WithCompanyId(null);

        @event.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "CompanyId defaults to empty and visibility defaults to false")]
    public void Defaults_are_empty_and_false()
    {
        var @event = new EmployeeChangedEvent();

        @event.CompanyId.Should().BeEmpty();
        @event.IsVisibleAcrossCompanies.Should().BeFalse();
        @event.GrantCompanyId.Should().BeNull();
    }

    [Fact(DisplayName = "WithGrantCompanyId trims and clears blank")]
    public void WithGrantCompanyId_trims_and_clears_blank()
    {
        var @event = new EmployeeChangedEvent()
            .WithGrantCompanyId("  company-grant  ");

        @event.GrantCompanyId.Should().Be("company-grant");

        @event.WithGrantCompanyId("  ");
        @event.GrantCompanyId.Should().BeNull();
    }

    [Fact(DisplayName = "3-arg WithLocation leaves geography unresolved")]
    public void Three_arg_WithLocation_leaves_geography_unresolved()
    {
        var locationId = Guid.NewGuid();

        var @event = new EmployeeChangedEvent().WithLocation(locationId, "Accra Office", "ACC");

        @event.LocationId.Should().Be(locationId);
        @event.LocationName.Should().Be("Accra Office");
        @event.LocationCode.Should().Be("ACC");
        @event.LocationGeographyResolved.Should().BeFalse();
        @event.LocationCountry.Should().BeNull();
        @event.LocationRegion.Should().BeNull();
    }

    [Fact(DisplayName = "5-arg WithLocation marks geography resolved")]
    public void Five_arg_WithLocation_marks_geography_resolved()
    {
        var locationId = Guid.NewGuid();

        var @event = new EmployeeChangedEvent()
            .WithLocation(locationId, "Accra Office", "ACC", "GH", "Greater Accra");

        @event.LocationCountry.Should().Be("GH");
        @event.LocationRegion.Should().Be("Greater Accra");
        @event.LocationGeographyResolved.Should().BeTrue();
    }

    [Fact(DisplayName = "5-arg WithLocation with a null country still marks geography resolved")]
    public void Five_arg_WithLocation_with_null_country_marks_geography_resolved()
    {
        var @event = new EmployeeChangedEvent()
            .WithLocation(Guid.NewGuid(), "Remote", "RMT", null, null);

        @event.LocationCountry.Should().BeNull();
        @event.LocationRegion.Should().BeNull();
        @event.LocationGeographyResolved.Should().BeTrue();
    }
}
