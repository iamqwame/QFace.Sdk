namespace QimErp.Shared.Common.AppSettings.Helpers;

public static class StructuredSettingsHelpers
{
    public static string Get(Dictionary<string, string> values, string key, string defaultValue) =>
        values.TryGetValue(key, out var value) ? value : defaultValue;

    public static bool GetBool(Dictionary<string, string> values, string key, bool defaultValue) =>
        values.TryGetValue(key, out var value)
            ? bool.TryParse(value, out var parsed) ? parsed : defaultValue
            : defaultValue;

    public static int GetInt(Dictionary<string, string> values, string key, int defaultValue) =>
        values.TryGetValue(key, out var value)
            ? int.TryParse(value, out var parsed) ? parsed : defaultValue
            : defaultValue;
}
