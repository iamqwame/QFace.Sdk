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

    // ── Leadership photos ───────────────────────────────────────────────────
    // Two stable, hand-picked headshots for the top two demo employees (admin /
    // tenant creator at index 0, second-most-senior leader at index 1). Every
    // other employee falls back to null so the frontend renders initials —
    // randomuser.me and xsgames.co URLs were 404'ing or blocked in production
    // and produced inconsistent UX.
    //
    // These are placeholder Unsplash URLs (CDN-hosted, stable, no API key
    // required) intended to be swapped for the company's actual leadership
    // headshots once the customer provides them. Keep the same constant names
    // so the swap is a one-line change.
    public static readonly string AdminLeaderPhoto =
        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&q=80";
    public static readonly string SecondLeaderPhoto =
        "https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=400&q=80";

    /// <summary>
    /// Returns one of two hand-picked leadership headshot URLs for the top two
    /// employees of a demo seed batch (index 0 → admin / CEO, index 1 → second
    /// senior leader e.g. Chairman / CFO). Any other index returns <c>null</c>,
    /// which the caller should treat as "leave ProfilePicture unset" so the
    /// frontend Avatar component falls back to the employee's initials.
    /// </summary>
    /// <remarks>
    /// The two URLs are stored in <see cref="AdminLeaderPhoto"/> and
    /// <see cref="SecondLeaderPhoto"/>. They are placeholder Unsplash CDN URLs
    /// that should be replaced with the company's actual two leadership
    /// headshots once available.
    /// </remarks>
    public static string? PickLeadershipPhoto(int employeeIndex) => employeeIndex switch
    {
        0 => AdminLeaderPhoto,
        1 => SecondLeaderPhoto,
        _ => null
    };

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
