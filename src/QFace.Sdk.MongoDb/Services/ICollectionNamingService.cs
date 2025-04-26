namespace QFace.Sdk.MongoDb.Services;

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