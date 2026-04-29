using Bogus;
using QimErp.Shared.DemoData.Bogus;

namespace QimErp.Shared.DemoData.Media;

/// <summary>
/// Curated catalogue of CC-licensed portrait URLs used by the demo-employee faker
/// when assigning a <c>ProfilePicture</c>. Centralising the URLs lets us swap the
/// pool out (e.g. for hand-curated Ghanaian/African portraits hosted under
/// <c>qimerp-saas-frontend/public/demo-portraits/</c>) without touching the faker.
///
/// v1 pool — xsgames.co/randomusers CC0 portraits, indices 1..78 per gender. They
/// resolve, are distinct, and are explicitly free for any use, so they are safe
/// for an MIT-licensed library. They are NOT specifically Ghanaian; the follow-up
/// task is to replace each entry with a hand-picked African/Ghanaian portrait
/// hosted by us. The shape of this class stays the same when that swap happens —
/// only the two arrays below change.
/// </summary>
public static class PortraitCatalog
{
    private const string XsgamesBase = "https://xsgames.co/randomusers/assets/avatars";
    private const int XsgamesIndexCount = 78;

    private static readonly string[] _malePortraits = BuildXsgamesPool("male");
    private static readonly string[] _femalePortraits = BuildXsgamesPool("female");

    /// <summary>Male portrait URL pool. 78 entries in v1.</summary>
    public static IReadOnlyList<string> MalePortraits => _malePortraits;

    /// <summary>Female portrait URL pool. 78 entries in v1.</summary>
    public static IReadOnlyList<string> FemalePortraits => _femalePortraits;

    /// <summary>
    /// Picks one portrait URL appropriate for the given gender using the supplied
    /// <see cref="Faker"/> so selection participates in deterministic seeding.
    /// </summary>
    public static string PickPortrait(Faker faker, GhanaFakerExtensions.Gender gender)
    {
        ArgumentNullException.ThrowIfNull(faker);

        var pool = gender == GhanaFakerExtensions.Gender.Male
            ? _malePortraits
            : _femalePortraits;

        return faker.Random.ListItem(pool);
    }

    private static string[] BuildXsgamesPool(string genderPath)
    {
        var pool = new string[XsgamesIndexCount];
        for (var i = 0; i < XsgamesIndexCount; i++)
        {
            pool[i] = $"{XsgamesBase}/{genderPath}/{i + 1}.jpg";
        }
        return pool;
    }
}
