namespace QimErp.Shared.DemoData.Media;

// Real-photo URLs only. Vector / generated avatars (DiceBear, ui-avatars,
// Pravatar, Robohash) are deliberately excluded so every employee gets a face.
// Curated set of stable Unsplash CDN headshots — randomuser.me and xsgames.co
// were intermittently 404'ing in production, so the pools were swapped for
// hand-picked Unsplash photo IDs that mix age, ethnicity, attire and
// head-and-shoulder framing.
public static class PortraitCatalog
{
    private const string UnsplashUrlFormat =
        "https://images.unsplash.com/photo-{0}?w=400&q=80&fit=crop&crop=faces";

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

    /// <summary>
    /// Returns a portrait URL for the employee at <paramref name="employeeIndex" /> in the seeded list.
    /// Index 0 → AdminLeaderPhoto (admin / tenant creator).
    /// Index 1 → SecondLeaderPhoto (second senior leader).
    /// Index ≥ 2 → 70% chance of returning a deterministic-by-(seed, index) URL from the matching gender pool, 30% chance of returning null (frontend renders initials).
    /// Determinism: same (seed, index, gender) tuple always returns the same URL — so a re-seed with the same RandomSeed produces the same photo distribution.
    /// </summary>
    public static string? PickPortraitForEmployee(int employeeIndex, GhanaFakerExtensions.Gender gender, int seed)
    {
        if (employeeIndex == 0) return AdminLeaderPhoto;
        if (employeeIndex == 1) return SecondLeaderPhoto;

        var rng = new Random(HashCode.Combine(seed, employeeIndex));
        if (rng.NextDouble() < 0.30) return null;

        var pool = gender == GhanaFakerExtensions.Gender.Female ? _femalePortraits : _malePortraits;
        return pool[rng.Next(pool.Length)];
    }

    private static string[] BuildMalePool()
    {
        // Curated Unsplash photo IDs for male headshots — diverse ages,
        // ethnicities, professional attire, head-and-shoulder framing.
        // Duplicates removed from the source list.
        var photoIds = new[]
        {
            "1507003211169-0a1dd7228f2d",
            "1500648767791-00dcc994a43e",
            "1506794778202-cad84cf45f1d",
            "1472099645785-5658abf4ff4e",
            "1519085360753-af0119f7cbe7",
            "1568602471122-7832951cc4c5",
            "1531427186611-ecfd6d936c79",
            "1463453091185-61582044d556",
            "1502323777036-f29e3972d82f",
            "1542178243-bc20204b769f",
            "1552058544-f2b08422138a",
            "1488161628813-04466f872be2",
            "1463725506904-04a3548e8a0c",
            "1542909168-82c3e7fdca5c",
            "1542909192-2f2241a0d8c3",
            "1546961342-c98c91dc8fcf",
            "1474176857210-7287d38d27c6",
            "1492447216082-4726bf04d1fb",
            "1521119989659-a83eee488004",
            "1500048993953-d23a436266cf",
            "1556157382-97eda2d62296",
            "1539571696857-5a6c4b1bd3a8",
            "1607990281513-2c110a25bd8c",
            "1545167622-3a6ac756afa4",
            "1528892952291-009c663ce843",
            "1504593811423-6dd665756598",
            "1521572163474-6864f9cf17ab",
            "1438761681033-6461ffad8d80",
            "1463100099107-aa0980c362e6",
        };

        return photoIds.Select(id => string.Format(UnsplashUrlFormat, id)).ToArray();
    }

    private static string[] BuildFemalePool()
    {
        // Curated Unsplash photo IDs for female headshots — diverse ages,
        // ethnicities, professional attire, head-and-shoulder framing.
        // Duplicates removed from the source list.
        var photoIds = new[]
        {
            "1573497019940-1c28c88b4f3e",
            "1438761681033-6461ffad8d80",
            "1494790108377-be9c29b29330",
            "1531123897727-8f129e1688ce",
            "1487412720507-e7ab37603c6f",
            "1502823403499-6ccfcf4fb453",
            "1573496359142-b8d87734a5a2",
            "1521252659862-eec69941b71f",
            "1517841905240-472988babdf9",
            "1517363898874-737b62a7db91",
            "1620577751996-2d2d8e26e5db",
            "1502685104226-ee32379fefbe",
            "1573497620053-ea5300f94f21",
            "1539571696857-5a6c4b1bd3a8",
            "1554151228-14d9def656e4",
            "1546961342-c98c91dc8fcf",
            "1518082593638-b4a1ac28b94d",
            "1519085360753-af0119f7cbe7",
            "1531746020798-e6953c6e8e04",
            "1551836022-d5d88e9218df",
            "1607746882042-944635dfe10e",
            "1593104547489-5cfb3839a3b5",
            "1488426862026-3ee34a7d66df",
            "1525134479668-1bee5c7c6845",
            "1559963911-46569d80b73f",
            "1485875437342-9b39470b3d95",
            "1541823709867-1b206113eafd",
        };

        return photoIds.Select(id => string.Format(UnsplashUrlFormat, id)).ToArray();
    }
}
