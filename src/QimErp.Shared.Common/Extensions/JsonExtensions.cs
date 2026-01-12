using System.Text.Json.Serialization;

namespace QimErp.Shared.Common.Extensions;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateOnly.TryParseExact(dateString, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            
            // Try parsing with other common formats
            if (DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }
        }
        
        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateOnly.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}

public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (dateString.IsEmpty())
            {
                return null;
            }

            if (DateOnly.TryParseExact(dateString, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            
            // Try parsing with other common formats
            if (DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }
        }
        
        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateOnly?.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class JsonbListConverter<T> : ValueConverter<List<T>, string>
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public JsonbListConverter() : base(
        // Convert List<T> to string (for database storage)
        v => JsonSerializer.Serialize(v, JsonOpts),
        
        // Convert string to List<T> (when reading from database)
        v => string.IsNullOrWhiteSpace(v)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(v, JsonOpts) ?? new List<T>())
    {
    }
}

public class SafeJsonbListConverter<T> : ValueConverter<List<T>, string>
{
    public SafeJsonbListConverter() : base(
        // Convert List<T> to string (for database storage)
        // Use static helper to avoid optional parameter issues in expression trees
        v => SafeSerializeList(v),
        
        // Convert string to List<T> (when reading from database)
        // Use a static helper method that can be called from expression tree
        v => SafeDeserializeList(v))
    {
    }
    
    private static string SafeSerializeList(List<T>? value)
    {
        if (value == null)
            return string.Empty;
        
        return value.Serialize();
    }
    
    private static List<T> SafeDeserializeList(string? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value))
            return new List<T>();
        
        try
        {
            return value.Deserialize<List<T>>() ?? new List<T>();
        }
        catch (JsonException)
        {
            // If JSON is invalid, return empty list instead of throwing
            return new List<T>();
        }
    }
}