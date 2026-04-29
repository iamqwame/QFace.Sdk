using System.Text.RegularExpressions;
using Bogus;
using FluentAssertions;
using QimErp.Shared.DemoData.Bogus;
using QimErp.Shared.DemoData.Ghana;
using Xunit;

namespace QimErp.Shared.DemoData.Tests;

public class GhanaFakerExtensionsTests
{
    private static Faker NewFaker(int seed = 12345) =>
        new Faker { Random = new Randomizer(seed) };

    [Fact]
    public void GhanaPhone_AlwaysStartsWithPlus233()
    {
        var faker = NewFaker();
        var pattern = new Regex(@"^\+233\d{9}$");

        for (var i = 0; i < 100; i++)
        {
            var phone = faker.GhanaPhone();
            pattern.IsMatch(phone).Should().BeTrue($"phone '{phone}' should match +233 + 9 digits");
        }
    }

    [Fact]
    public void GhanaPhone_PrefixIsValidCarrier()
    {
        var faker = NewFaker();
        var validPrefixes = new HashSet<string>(GhanaTelecom.AllPrefixes);

        for (var i = 0; i < 100; i++)
        {
            var phone = faker.GhanaPhone();
            // "+233XXYYYYYYY" — prefix is at indices 4..6 (chars 5-6 in 1-based terms).
            var prefix = phone.Substring(4, 2);
            validPrefixes.Should().Contain(prefix, $"prefix '{prefix}' from '{phone}' should be a known Ghana carrier prefix");
        }
    }

    [Fact]
    public void GhanaCard_MatchesFormat()
    {
        var faker = NewFaker();
        var pattern = new Regex(@"^GHA-\d{9}-\d$");

        for (var i = 0; i < 50; i++)
        {
            var card = faker.GhanaCard();
            pattern.IsMatch(card).Should().BeTrue($"Ghana card '{card}' should match GHA-#########-#");
        }
    }

    [Fact]
    public void GhanaSsnit_StartsWithLetterThenTwelveDigits()
    {
        var faker = NewFaker();
        var pattern = new Regex(@"^[A-Z]\d{12}$");

        for (var i = 0; i < 50; i++)
        {
            var ssnit = faker.GhanaSsnit();
            pattern.IsMatch(ssnit).Should().BeTrue($"SSNIT '{ssnit}' should match [A-Z] + 12 digits");
        }
    }

    [Fact]
    public void GhanaTin_StartsWithPThenTenDigits()
    {
        var faker = NewFaker();
        var pattern = new Regex(@"^P\d{10}$");

        for (var i = 0; i < 50; i++)
        {
            var tin = faker.GhanaTin();
            pattern.IsMatch(tin).Should().BeTrue($"TIN '{tin}' should match P + 10 digits");
        }
    }

    [Fact]
    public void GhanaCity_NoRegion_PicksFromAnyRegion()
    {
        var faker = NewFaker();
        var allCities = new HashSet<string>(
            GhanaGeography.CitiesByRegion.Values.SelectMany(c => c));

        for (var i = 0; i < 100; i++)
        {
            var city = faker.GhanaCity();
            allCities.Should().Contain(city, $"city '{city}' should be in the union of all region city lists");
        }
    }

    [Fact]
    public void GhanaCity_GivenRegion_PicksFromThatRegion()
    {
        var faker = NewFaker();
        const string region = "Greater Accra";
        var allowed = new HashSet<string>(GhanaGeography.CitiesByRegion[region]);

        for (var i = 0; i < 100; i++)
        {
            var city = faker.GhanaCity(region);
            allowed.Should().Contain(city, $"city '{city}' should belong to '{region}'");
        }
    }

    [Fact]
    public void GhanaFirstName_ReturnsNonEmpty()
    {
        var faker = NewFaker();

        var male = faker.GhanaFirstName(GhanaFakerExtensions.Gender.Male);
        var female = faker.GhanaFirstName(GhanaFakerExtensions.Gender.Female);

        male.Should().NotBeNullOrWhiteSpace();
        female.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GhanaBank_ReturnsCommercialBankNotBoG()
    {
        var faker = NewFaker();

        for (var i = 0; i < 50; i++)
        {
            var bank = faker.GhanaBank();
            bank.Name.Should().NotBe("Bank of Ghana");
        }
    }
}
