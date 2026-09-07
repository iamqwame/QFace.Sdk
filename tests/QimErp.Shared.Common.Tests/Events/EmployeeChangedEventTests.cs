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
}
