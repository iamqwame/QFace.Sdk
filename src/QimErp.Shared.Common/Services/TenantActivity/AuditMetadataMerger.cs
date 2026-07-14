using System.Text.Json;
using QFace.Sdk.Extensions;

namespace QimErp.Shared.Common.Services.TenantActivity;

/// <summary>
/// Merges request-context audit fields into caller-supplied metadata without overwriting business keys.
/// </summary>
public static class AuditMetadataMerger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string? Merge(string? metadataJson, AuditRequestContext? requestContext)
    {
        if (requestContext is null)
        {
            return metadataJson;
        }

        var metadata = ParseMetadata(metadataJson);

        TryAdd(metadata, "ipAddress", requestContext.IpAddress);
        TryAdd(metadata, "userAgent", requestContext.UserAgent);
        TryAdd(metadata, "sessionId", requestContext.SessionId);

        return metadata.Count == 0 ? metadataJson : metadata.Serialize(JsonOptions);
    }

    private static Dictionary<string, object?> ParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metadataJson, JsonOptions);
            if (parsed is null || parsed.Count == 0)
            {
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }

            return parsed.ToDictionary(
                pair => pair.Key,
                pair => (object?)ElementToObject(pair.Value),
                StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void TryAdd(Dictionary<string, object?> metadata, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!metadata.ContainsKey(key) || metadata[key] is null or "")
        {
            metadata[key] = value;
        }
    }

    private static object? ElementToObject(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
}
