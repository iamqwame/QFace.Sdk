namespace QFace.Sdk.MongoDb.Services;

/// <summary>
/// Interface for factory that creates MongoDB client instances
/// </summary>
public interface IMongoDbClientFactory
{
    /// <summary>
    /// Gets a MongoDB client
    /// </summary>
    /// <returns>MongoDB client</returns>
    IMongoClient GetClient();
    
    /// <summary>
    /// Gets a MongoDB database
    /// </summary>
    /// <param name="databaseName">Optional database name (overrides configuration)</param>
    /// <returns>MongoDB database</returns>
    IMongoDatabase GetDatabase(string databaseName = null);
}