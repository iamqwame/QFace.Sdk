namespace QimErp.Shared.Common.AppSettings.Seeding;

public static class AppSettingsSeeder
{
    public static async Task<int> SeedMissingAsync(
        DbSet<AppSetting> appSettings,
        IReadOnlyList<AppSettingSeedEntry> catalogue,
        CancellationToken cancellationToken = default,
        Action<AppSetting>? configure = null)
    {
        if (catalogue.Count == 0)
            return 0;

        var allKeys = catalogue.Select(c => c.Key).ToList();
        var existing = await appSettings
            .Where(s => allKeys.Contains(s.Key))
            .Select(s => s.Key)
            .ToListAsync(cancellationToken);

        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var toInsert = new List<AppSetting>();

        foreach (var entry in catalogue.Where(c => !existingSet.Contains(c.Key)))
        {
            var setting = entry.Value switch
            {
                string s => AppSetting.Create(entry.Key, s, entry.Category),
                string[] a => AppSetting.CreateArray(entry.Key, a, entry.Category),
                _ => AppSetting.CreateObject(entry.Key, entry.Value, entry.Category),
            };

            if (!string.IsNullOrWhiteSpace(entry.Description))
                setting.WithDescription(entry.Description);

            configure?.Invoke(setting);
            toInsert.Add(setting);
        }

        if (toInsert.Count == 0)
            return 0;

        await appSettings.AddRangeAsync(toInsert, cancellationToken);
        return toInsert.Count;
    }
}
