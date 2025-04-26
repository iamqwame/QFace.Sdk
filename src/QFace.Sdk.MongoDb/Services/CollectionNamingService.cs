namespace QFace.Sdk.MongoDb.Services;

/// <summary>
/// Service for determining MongoDB collection names based on configuration
/// </summary>
public class CollectionNamingService : ICollectionNamingService
{
    private readonly CollectionNamingOptions _options;
    
    public CollectionNamingService(IOptions<MongoDbOptions> options)
    {
        _options = options.Value.CollectionNaming;
    }
    
    /// <summary>
    /// Gets the collection name for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The collection name</returns>
    public string GetCollectionName<T>()
    {
        return GetCollectionName(typeof(T).Name);
    }
    
    /// <summary>
    /// Gets the collection name for a type name based on configured naming strategy
    /// </summary>
    /// <param name="typeName">The type name</param>
    /// <returns>The collection name</returns>
    public string GetCollectionName(string typeName)
    {
        // Clean up the type name (e.g., remove "Document" suffix if present)
        var cleanName = typeName;
        if (cleanName.EndsWith("Document"))
        {
            cleanName = cleanName.Substring(0, cleanName.Length - 8);
        }
        
        var collectionName = _options.Strategy.ToLowerInvariant() switch
        {
            "plural" => Pluralize(cleanName),
            "camelcase" => ToCamelCase(cleanName),
            "pluralcamelcase" => ToCamelCase(Pluralize(cleanName)),
            _ => cleanName // Raw strategy
        };
        
        // Apply lowercase if configured
        return _options.ForceLowerCase ? collectionName.ToLowerInvariant() : collectionName;
    }
    
    /// <summary>
    /// Converts a string to camel case
    /// </summary>
    private string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
    
    /// <summary>
    /// Pluralizes a word using simple English pluralization rules
    /// </summary>
    private string Pluralize(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }
        
        // Simple pluralization rules - extend as needed
        if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase))
        {
            // Check if the letter before 'y' is a vowel
            if (word.Length > 1 && !IsVowel(word[word.Length - 2]))
            {
                return word.Substring(0, word.Length - 1) + "ies";
            }
        }
        
        if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) || 
            word.EndsWith("x", StringComparison.OrdinalIgnoreCase) || 
            word.EndsWith("z", StringComparison.OrdinalIgnoreCase) || 
            word.EndsWith("ch", StringComparison.OrdinalIgnoreCase) || 
            word.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return word + "es";
        }
        
        return word + "s";
    }
    
    /// <summary>
    /// Checks if a character is a vowel
    /// </summary>
    private bool IsVowel(char c)
    {
        return "aeiou".Contains(char.ToLowerInvariant(c));
    }
}

/// <summary>
/// Interface for service that determines MongoDB collection names
/// </summary>
public interface ICollectionNamingService
{
    /// <summary>
    /// Gets the collection name for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The collection name</returns>
    string GetCollectionName<T>();
    
    /// <summary>
    /// Gets the collection name for a type name based on configured naming strategy
    /// </summary>
    /// <param name="typeName">The type name</param>
    /// <returns>The collection name</returns>
    string GetCollectionName(string typeName);
}