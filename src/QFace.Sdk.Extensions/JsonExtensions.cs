using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace QFace.Sdk.Extensions;

/// <summary>
/// Extension methods for JSON serialization and deserialization
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Serializes an object to a JSON string using System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">Optional JsonSerializerOptions.</param>
    /// <returns>Serialized JSON string.</returns>
    public static string Serialize<T>(this T value, JsonSerializerOptions? options = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T using System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional JsonSerializerOptions.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static T? Deserialize<T>(this string json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to a JSON string using Newtonsoft.Json.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="settings">Optional JsonSerializerSettings.</param>
    /// <returns>Serialized JSON string.</returns>
    public static string SerializeWithNewtonsoft<T>(this T value, JsonSerializerSettings? settings = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return JsonConvert.SerializeObject(value, settings ?? DefaultNewtonsoftSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T using Newtonsoft.Json.
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="settings">Optional JsonSerializerSettings.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static T? DeserializeWithNewtonsoft<T>(this string json, JsonSerializerSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
        return JsonConvert.DeserializeObject<T>(json, settings ?? DefaultNewtonsoftSettings);
    }

    /// <summary>
    /// Default JsonSerializerOptions for System.Text.Json.
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Default JsonSerializerSettings for Newtonsoft.Json.
    /// </summary>
    private static readonly JsonSerializerSettings DefaultNewtonsoftSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
    
    /// <summary>
    /// Attempts to deserialize a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="result">When this method returns, contains the deserialized object or the default value of T if deserialization failed.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryDeserialize<T>(this string json, out T? result)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            result = default;
            return false;
        }

        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
    
    /// <summary>
    /// Clones an object by serializing and deserializing it.
    /// </summary>
    /// <typeparam name="T">Type of the object to clone.</typeparam>
    /// <param name="source">The object to clone.</param>
    /// <returns>A deep clone of the object.</returns>
    public static T? DeepClone<T>(this T source)
    {
        if (source == null) return default;
        
        var json = JsonSerializer.Serialize(source, DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }
}