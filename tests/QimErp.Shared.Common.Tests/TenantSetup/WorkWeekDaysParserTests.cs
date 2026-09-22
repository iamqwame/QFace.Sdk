using FluentAssertions;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantSetup;

public sealed class WorkWeekDaysParserTests
{
    private static readonly DayOfWeek[] MondayToFriday =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
    ];

    [Theory(DisplayName = "Parse falls back to Mon-Fri for null, empty and whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_falls_back_for_absent_input(string? pattern)
        => WorkWeekDaysParser.Parse(pattern).Should().BeEquivalentTo(MondayToFriday);

    [Fact(DisplayName = "Parse falls back to Mon-Fri when nothing in the pattern is recognised")]
    public void Parse_falls_back_for_unparseable_input()
        => WorkWeekDaysParser.Parse("M/W/F").Should().BeEquivalentTo(MondayToFriday);

    [Fact(DisplayName = "Parse keeps the recognised segments and skips the rest")]
    public void Parse_skips_unrecognised_segments()
        => WorkWeekDaysParser.Parse("Mon,Blursday,Fri")
            .Should().BeEquivalentTo([DayOfWeek.Monday, DayOfWeek.Friday]);

    [Fact(DisplayName = "Parse expands a day range inclusively")]
    public void Parse_expands_ranges()
        => WorkWeekDaysParser.Parse("Mon-Fri").Should().BeEquivalentTo(MondayToFriday);

    [Fact(DisplayName = "Parse expands a range that wraps past Sunday")]
    public void Parse_expands_wrapping_ranges()
        => WorkWeekDaysParser.Parse("Sat-Mon")
            .Should().BeEquivalentTo([DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday]);

    [Fact(DisplayName = "Parse combines ranges and single days, case-insensitively")]
    public void Parse_combines_ranges_and_singles()
        => WorkWeekDaysParser.Parse("mon-wed, SATURDAY")
            .Should().BeEquivalentTo([DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday]);

    [Theory(DisplayName = "TryParse accepts patterns whose every segment is a known day token")]
    [InlineData("Mon-Fri")]
    [InlineData("Mon,Wed,Fri")]
    [InlineData("Monday, Tuesday")]
    [InlineData("sun")]
    [InlineData("Mon-Wed,Sat")]
    public void TryParse_accepts_valid_patterns(string pattern)
    {
        WorkWeekDaysParser.TryParse(pattern, out var days).Should().BeTrue();
        days.Should().NotBeEmpty();
    }

    [Theory(DisplayName = "TryParse rejects absent input and any unrecognised segment")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("M/W/F")]
    [InlineData("Mon,Blursday,Fri")]
    [InlineData("Mon-Funday")]
    [InlineData("Noneday-Fri")]
    public void TryParse_rejects_invalid_patterns(string? pattern)
    {
        WorkWeekDaysParser.TryParse(pattern, out var days).Should().BeFalse();
        days.Should().BeEmpty();
    }

    [Fact(DisplayName = "IsValid tracks TryParse")]
    public void IsValid_tracks_TryParse()
    {
        WorkWeekDaysParser.IsValid("Mon,Wed,Fri").Should().BeTrue();
        WorkWeekDaysParser.IsValid("M/W/F").Should().BeFalse();
    }
}
