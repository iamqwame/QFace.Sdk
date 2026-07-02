namespace QimErp.Shared.Common.AppSettings.Seeding;

public sealed record AppSettingSeedEntry(
    string Key,
    object Value,
    string Category,
    string? Description = null);
