using GhanaData = QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Bogus;

public static class GhanaFakerExtensions
{
    public enum Gender { Male, Female }

    public static string GhanaFirstName(this Faker f, Gender gender) =>
        gender == Gender.Male
            ? f.Random.ListItem(GhanaData.GhanaPersonNames.MaleFirstNames.AsList())
            : f.Random.ListItem(GhanaData.GhanaPersonNames.FemaleFirstNames.AsList());

    public static string GhanaMiddleName(this Faker f, Gender gender) =>
        gender == Gender.Male
            ? f.Random.ListItem(GhanaData.GhanaPersonNames.MaleMiddleNames.AsList())
            : f.Random.ListItem(GhanaData.GhanaPersonNames.FemaleMiddleNames.AsList());

    public static string GhanaLastName(this Faker f) =>
        f.Random.ListItem(GhanaData.GhanaPersonNames.LastNames.AsList());

    public static string GhanaFullName(this Faker f, Gender gender) =>
        $"{f.GhanaFirstName(gender)} {f.GhanaLastName()}";

    public static string GhanaFullNameWithMiddle(this Faker f, Gender gender) =>
        $"{f.GhanaFirstName(gender)} {f.GhanaMiddleName(gender)} {f.GhanaLastName()}";

    private static readonly PhoneNumbers.PhoneNumberUtil _phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

    // Phones must pass libphonenumber Ghana validation, since the consumer entity
    // layer normalises through the same library and silently drops anything that
    // doesn't parse. Retry up to 8 times before falling back to a known-good MTN canary.
    public static string GhanaPhone(this Faker f, GhanaData.GhanaTelecom.Carrier? carrier = null)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var resolvedCarrier = carrier ?? PickCarrier(f);
            var prefixes = GhanaData.GhanaTelecom.PrefixesFor(resolvedCarrier);
            var prefix = f.Random.ListItem(prefixes.AsList());
            var candidate = $"+233{prefix}{f.Random.Number(1_000_000, 9_999_999)}";
            try
            {
                var parsed = _phoneUtil.Parse(candidate, "GH");
                if (_phoneUtil.IsValidNumber(parsed)) return candidate;
            }
            catch (PhoneNumbers.NumberParseException) { }
        }
        return "+233241234567";
    }

    private static GhanaData.GhanaTelecom.Carrier PickCarrier(Faker f)
    {
        var roll = f.Random.Double();
        return roll switch
        {
            < 0.60 => GhanaData.GhanaTelecom.Carrier.MTN,
            < 0.80 => GhanaData.GhanaTelecom.Carrier.Telecel,
            _      => GhanaData.GhanaTelecom.Carrier.AT
        };
    }

    public static string GhanaRegion(this Faker f) =>
        f.Random.ListItem(GhanaData.GhanaGeography.Regions.AsList());

    public static string GhanaCity(this Faker f, string? region = null)
    {
        if (region != null && GhanaData.GhanaGeography.CitiesByRegion.TryGetValue(region, out var cities))
            return f.Random.ListItem(cities.AsList());
        var pickedRegion = f.Random.ListItem(GhanaData.GhanaGeography.CitiesByRegion.Keys.ToList());
        return f.Random.ListItem(GhanaData.GhanaGeography.CitiesByRegion[pickedRegion].AsList());
    }

    public static string GhanaStreet(this Faker f) =>
        $"{f.Random.ListItem(GhanaData.GhanaGeography.Streets.AsList())} {f.Random.Number(1, 9999)}";

    public static string GhanaGpsCode(this Faker f, string? region = null)
    {
        var resolved = region ?? f.Random.ListItem(GhanaData.GhanaGeography.Regions.AsList());
        var prefix = GhanaData.GhanaGeography.RegionGpsPrefix.TryGetValue(resolved, out var p) ? p : "GA";
        return $"{prefix}-{f.Random.Number(100, 999)}-{f.Random.Number(1000, 9999)}";
    }

    public static string GhanaCard(this Faker f) =>
        f.Random.Replace(GhanaData.GhanaIdentification.GhanaCardPattern);

    public static string GhanaSsnit(this Faker f)
    {
        var leadLetter = f.Random.Char('A', 'Z');
        return $"{leadLetter}{f.Random.ReplaceNumbers("############")}";
    }

    public static string GhanaTin(this Faker f) =>
        f.Random.Replace(GhanaData.GhanaIdentification.TinPattern);

    public static string GhanaPassport(this Faker f) =>
        f.Random.Replace(GhanaData.GhanaIdentification.PassportPattern);

    public static string GhanaDriverLicence(this Faker f)
    {
        var leftA  = f.Random.Char('A', 'Z');
        var leftB  = f.Random.Char('A', 'Z');
        var rightA = f.Random.Char('A', 'Z');
        var rightB = f.Random.Char('A', 'Z');
        return $"{leftA}{leftB}{f.Random.Number(10000, 99999)}{rightA}{rightB}";
    }

    public static string GhanaEthnicGroup(this Faker f) =>
        PickWeighted(f, GhanaData.GhanaEthnicity.Distribution);

    public static string GhanaReligionLabel(this Faker f) =>
        PickWeighted(f, GhanaData.GhanaReligion.Distribution);

    public static string GhanaDenominationFor(this Faker f, string religion) => religion switch
    {
        "Christian" => PickWeighted(f, GhanaData.GhanaReligion.ChristianDenominations),
        "Islam"     => PickWeighted(f, GhanaData.GhanaReligion.MuslimDenominations),
        _           => string.Empty
    };

    public static GhanaData.GhanaBanks.Bank GhanaBank(this Faker f) =>
        f.Random.ListItem(GhanaData.GhanaBanks.CommercialBanks.AsList());

    public static string GhanaAccountNumber(this Faker f) =>
        f.Random.ReplaceNumbers("#############");

    private static string PickWeighted(Faker f, IReadOnlyList<(string Value, double Weight)> distribution)
    {
        var total = distribution.Sum(x => x.Weight);
        var roll = f.Random.Double(0, total);
        var cumulative = 0.0;
        foreach (var (value, weight) in distribution)
        {
            cumulative += weight;
            if (roll <= cumulative) return value;
        }
        return distribution[^1].Value;
    }

    // Bogus's Randomizer.ListItem wants IList<T>; our data exposes IReadOnlyList<T>.
    // Cache the materialised list per source so we don't allocate on every pick.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, object> _listCache = new();

    private static IList<T> AsList<T>(this IReadOnlyList<T> source)
    {
        if (source is IList<T> direct) return direct;
        if (_listCache.TryGetValue(source, out var cached)) return (IList<T>)cached;
        var materialised = source.ToList();
        _listCache.Add(source, materialised);
        return materialised;
    }
}
