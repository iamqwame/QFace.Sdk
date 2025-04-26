namespace QFace.Sdk.MongoDb.Models;

/// <summary>
/// Base document class that all MongoDB documents should inherit from.
/// Provides common fields and functionality.
/// </summary>
public abstract class BaseDocument
{
    /// <summary>
    /// The document's unique identifier
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Timestamp of when the document was created
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identifier of the user who created the document
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of when the document was last modified
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identifier of the user who last modified the document
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;

    
    /// <summary>
    /// Flag indicating whether the document is active or soft-deleted
    /// </summary>
    public bool IsActive { get; set; } = true;
}