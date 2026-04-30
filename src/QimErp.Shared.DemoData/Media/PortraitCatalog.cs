using Bogus;
using QimErp.Shared.DemoData.Bogus;

namespace QimErp.Shared.DemoData.Media;

// Real-photo URLs only. Vector / generated avatars (DiceBear, ui-avatars,
// Pravatar, Robohash) are deliberately excluded so every employee gets a face.
// Mixes randomuser.me (99/gender) and xsgames.co (78/gender) — same sources
// the Cal Bank reference spreadsheet uses.
public static class PortraitCatalog
{
    private const string RandomUserBase = "https://randomuser.me/api/portraits";
    private const string XsgamesBase    = "https://xsgames.co/randomusers/assets/avatars";
    private const int RandomUserIndexCount = 99;
    private const int XsgamesIndexCount    = 78;

    private static readonly string[] _malePortraits = BuildMalePool();
    private static readonly string[] _femalePortraits = BuildFemalePool();

    public static IReadOnlyList<string> MalePortraits => _malePortraits;
    public static IReadOnlyList<string> FemalePortraits => _femalePortraits;

    public static string PickPortrait(Faker faker, GhanaFakerExtensions.Gender gender)
    {
        ArgumentNullException.ThrowIfNull(faker);
        var pool = gender == GhanaFakerExtensions.Gender.Male ? _malePortraits : _femalePortraits;
        return faker.Random.ListItem(pool);
    }

    private static string[] BuildMalePool()
    {
        var pool = new List<string>(RandomUserIndexCount + XsgamesIndexCount);
        for (var i = 1; i <= RandomUserIndexCount; i++) pool.Add($"{RandomUserBase}/men/{i}.jpg");
        for (var i = 1; i <= XsgamesIndexCount; i++) pool.Add($"{XsgamesBase}/male/{i}.jpg");
        return pool.ToArray();
    }

    private static string[] BuildFemalePool()
    {
        var pool = new List<string>(RandomUserIndexCount + XsgamesIndexCount);
        for (var i = 1; i <= RandomUserIndexCount; i++) pool.Add($"{RandomUserBase}/women/{i}.jpg");
        for (var i = 1; i <= XsgamesIndexCount; i++) pool.Add($"{XsgamesBase}/female/{i}.jpg");
        return pool.ToArray();
    }
}
