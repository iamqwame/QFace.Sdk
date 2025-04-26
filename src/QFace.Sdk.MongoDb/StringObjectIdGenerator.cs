namespace QFace.Sdk.MongoDb;
/// <summary>
/// String ObjectId Generator for MongoDB
/// </summary>
public class StringObjectIdGenerator : IIdGenerator
{
    private static readonly StringObjectIdGenerator _instance = new();
    
    /// <summary>
    /// Gets the instance of the generator
    /// </summary>
    public static StringObjectIdGenerator Instance => _instance;
    
    /// <summary>
    /// Generates a new ID
    /// </summary>
    public object GenerateId(object container, object document)
    {
        return ObjectId.GenerateNewId().ToString();
    }
    
    /// <summary>
    /// Checks if the ID is empty
    /// </summary>
    public bool IsEmpty(object id)
    {
        return id == null || string.IsNullOrEmpty(id.ToString());
    }
}