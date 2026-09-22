namespace QimErp.Shared.Common.TenantSetup;

public static class WorkWeekDaysParser
{
    private static readonly Dictionary<string, DayOfWeek> DayTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Mon"] = DayOfWeek.Monday,
        ["Monday"] = DayOfWeek.Monday,
        ["Tue"] = DayOfWeek.Tuesday,
        ["Tuesday"] = DayOfWeek.Tuesday,
        ["Wed"] = DayOfWeek.Wednesday,
        ["Wednesday"] = DayOfWeek.Wednesday,
        ["Thu"] = DayOfWeek.Thursday,
        ["Thursday"] = DayOfWeek.Thursday,
        ["Fri"] = DayOfWeek.Friday,
        ["Friday"] = DayOfWeek.Friday,
        ["Sat"] = DayOfWeek.Saturday,
        ["Saturday"] = DayOfWeek.Saturday,
        ["Sun"] = DayOfWeek.Sunday,
        ["Sunday"] = DayOfWeek.Sunday,
    };

    /// <summary>The accepted day tokens, for error messages and UI hints.</summary>
    public static IReadOnlyCollection<string> AcceptedTokens { get; } = DayTokens.Keys.ToArray();

    public static IReadOnlyCollection<DayOfWeek> DefaultWorkWeek { get; } =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
    ];

    /// <summary>
    /// Read path. Unrecognised segments are skipped and a pattern that yields no day falls back to Mon-Fri,
    /// so a consumer never ends up with a zero-day work week. Use <see cref="TryParse"/> at a write boundary.
    /// </summary>
    public static HashSet<DayOfWeek> Parse(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return [.. DefaultWorkWeek];

        var result = ParseSegments(pattern, out _);

        return result.Count > 0 ? result : [.. DefaultWorkWeek];
    }

    /// <summary>
    /// Write path. Returns false for null, empty, or any pattern with a segment this vocabulary
    /// does not recognise — a typo must be rejected, not silently dropped by <see cref="Parse"/>.
    /// </summary>
    public static bool TryParse(string? pattern, out HashSet<DayOfWeek> days)
    {
        days = [];

        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var parsed = ParseSegments(pattern, out var hasUnrecognisedSegment);
        if (hasUnrecognisedSegment || parsed.Count == 0)
            return false;

        days = parsed;
        return true;
    }

    public static bool IsValid(string? pattern) => TryParse(pattern, out _);

    private static HashSet<DayOfWeek> ParseSegments(string pattern, out bool hasUnrecognisedSegment)
    {
        hasUnrecognisedSegment = false;
        var result = new HashSet<DayOfWeek>();

        foreach (var segment in pattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dashIndex = segment.IndexOf('-', StringComparison.Ordinal);
            if (dashIndex > 0 && dashIndex < segment.Length - 1)
            {
                if (!DayTokens.TryGetValue(segment[..dashIndex].Trim(), out var startDay) ||
                    !DayTokens.TryGetValue(segment[(dashIndex + 1)..].Trim(), out var endDay))
                {
                    hasUnrecognisedSegment = true;
                    continue;
                }

                var current = startDay;
                while (true)
                {
                    result.Add(current);
                    if (current == endDay) break;
                    current = (DayOfWeek)(((int)current + 1) % 7);
                }
            }
            else if (DayTokens.TryGetValue(segment.Trim(), out var singleDay))
            {
                result.Add(singleDay);
            }
            else
            {
                hasUnrecognisedSegment = true;
            }
        }

        return result;
    }
}
