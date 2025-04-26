namespace QFace.Sdk.MongoDb.Services;
/// <summary>
/// Interface for provider that accesses multiple MongoDB databases
/// </summary>
public interface IMongoDbProvider
{
    /// <summary>
    /// Gets a MongoDB database
    /// </summary>
    /// <param name="databaseName">The database name (if null, uses the default database)</param>
    /// <returns>The MongoDB database</returns>
    IMongoDatabase GetDatabase(string databaseName = null);
    
    
    /// <summary>
    /// Gets a MongoDB collection for a specific database
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="collectionName">The collection name</param>
    /// <param name="databaseName">The database name (optional)</param>
    /// <returns>The MongoDB collection</returns>
    IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName, string databaseName = null) where TDocument : BaseDocument;
    
   }
