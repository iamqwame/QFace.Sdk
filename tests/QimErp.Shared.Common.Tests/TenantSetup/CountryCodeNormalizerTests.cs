using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantSetup;

public sealed class CountryCodeNormalizerTests
{
    [Theory(DisplayName = "ISO-2 codes round-trip regardless of case")]
    [InlineData("GH", "GH")]
    [InlineData("gh", "GH")]
    [InlineData("NG", "NG")]
    [InlineData("ke", "KE")]
    [InlineData("tz", "TZ")]
    [InlineData("ZA", "ZA")]
    [InlineData("rw", "RW")]
    [InlineData("ZM", "ZM")]
    [InlineData("ug", "UG")]
    [InlineData("SN", "SN")]
    [InlineData("ci", "CI")]
    [InlineData("CM", "CM")]
    [InlineData("tg", "TG")]
    [InlineData("BF", "BF")]
    [InlineData("gb", "GB")]
    [InlineData("US", "US")]
    public void Iso_codes_round_trip(string input, string expected)
        => CountryCodeNormalizer.Normalize(input).Should().Be(expected);

    [Theory(DisplayName = "Country names and aliases resolve to ISO-2")]
    [InlineData("Ghana", "GH")]
    [InlineData("ghana", "GH")]
    [InlineData("Nigeria", "NG")]
    [InlineData("Kenya", "KE")]
    [InlineData("SOUTH AFRICA", "ZA")]
    [InlineData("south africa", "ZA")]
    [InlineData("Rwanda", "RW")]
    [InlineData("Tanzania", "TZ")]
    [InlineData("UNITED REPUBLIC OF TANZANIA", "TZ")]
    [InlineData("Zambia", "ZM")]
    [InlineData("Uganda", "UG")]
    [InlineData("Senegal", "SN")]
    [InlineData("CÔTE D'IVOIRE", "CI")]
    [InlineData("Côte d'Ivoire", "CI")]
    [InlineData("COTE D'IVOIRE", "CI")]
    [InlineData("IVORY COAST", "CI")]
    [InlineData("Cameroon", "CM")]
    [InlineData("Cameroun", "CM")]
    [InlineData("Togo", "TG")]
    [InlineData("Burkina Faso", "BF")]
    [InlineData("United Kingdom", "GB")]
    [InlineData("UK", "GB")]
    [InlineData("Great Britain", "GB")]
    [InlineData("England", "GB")]
    [InlineData("United States", "US")]
    [InlineData("United States of America", "US")]
    [InlineData("USA", "US")]
    public void Names_and_aliases_resolve(string input, string expected)
        => CountryCodeNormalizer.Normalize(input).Should().Be(expected);

    [Theory(DisplayName = "Dial codes and dial-code-plus-ISO shapes resolve to ISO-2")]
    [InlineData("+233", "GH")]
    [InlineData("+234", "NG")]
    [InlineData("+254", "KE")]
    [InlineData("+27", "ZA")]
    [InlineData("+250", "RW")]
    [InlineData("+255", "TZ")]
    [InlineData("+260", "ZM")]
    [InlineData("+256", "UG")]
    [InlineData("+221", "SN")]
    [InlineData("+225", "CI")]
    [InlineData("+237", "CM")]
    [InlineData("+228", "TG")]
    [InlineData("+226", "BF")]
    [InlineData("+44", "GB")]
    [InlineData("+1", "US")]
    [InlineData("+233-GH", "GH")]
    [InlineData("+234-NG", "NG")]
    [InlineData("+255-TZ", "TZ")]
    [InlineData("+225-CI", "CI")]
    [InlineData("+233-gh", "GH")]
    public void Dial_code_shapes_resolve(string input, string expected)
        => CountryCodeNormalizer.Normalize(input).Should().Be(expected);

    [Theory(DisplayName = "An explicit but unrecognised ISO suffix returns null instead of the dial code")]
    [InlineData("+233-XX")]
    [InlineData("+1-CA")]
    [InlineData("+44-IE")]
    public void Unrecognised_iso_suffix_returns_null(string input)
        => CountryCodeNormalizer.Normalize(input).Should().BeNull();

    [Theory(DisplayName = "Surrounding whitespace is trimmed")]
    [InlineData("  GH  ", "GH")]
    [InlineData("\tGhana\n", "GH")]
    [InlineData(" +233-GH ", "GH")]
    public void Whitespace_is_trimmed(string input, string expected)
        => CountryCodeNormalizer.Normalize(input).Should().Be(expected);

    [Theory(DisplayName = "Null, empty and whitespace return null")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_input_returns_null(string? input)
        => CountryCodeNormalizer.Normalize(input).Should().BeNull();

    [Theory(DisplayName = "Unknown input returns null")]
    [InlineData("XX")]
    [InlineData("Atlantis")]
    [InlineData("+999")]
    [InlineData("233")]
    [InlineData("+999-XX")]
    public void Unknown_input_returns_null(string input)
        => CountryCodeNormalizer.Normalize(input).Should().BeNull();

    [Fact(DisplayName = "Every registered country profile's code and name round-trip")]
    public void Registered_profiles_round_trip()
    {
        var profiles = new ServiceCollection()
            .AddLogging()
            .AddCountrySetupProfiles()
            .BuildServiceProvider()
            .GetServices<ICountrySetupProfile>()
            .ToList();

        profiles.Should().NotBeEmpty();

        foreach (var profile in profiles)
        {
            CountryCodeNormalizer.Normalize(profile.CountryCode)
                .Should().Be(profile.CountryCode.ToUpperInvariant(),
                    "profile code '{0}' must normalize to itself", profile.CountryCode);

            CountryCodeNormalizer.Normalize(profile.CountryName)
                .Should().Be(profile.CountryCode.ToUpperInvariant(),
                    "profile name '{0}' must normalize to '{1}'", profile.CountryName, profile.CountryCode);
        }
    }
}
