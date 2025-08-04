using OpenSearch.Client;

namespace QFace.Sdk.Elasticsearch.Models;

/// <summary>
/// Base document class that all Elasticsearch documents should inherit from.
/// Provides common fields and functionality.
/// </summary>
public abstract class EsBaseDocument
{
    /// <summary>
    /// The document's unique identifier
    /// </summary>
    [Keyword]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp of when the document was created
    /// </summary>
    [Date]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identifier of the user who created the document
    /// </summary>
    [Keyword]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of when the document was last modified
    /// </summary>
    [Date]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identifier of the user who last modified the document
    /// </summary>
    [Keyword]
    public string LastModifiedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Flag indicating whether the document is active or soft-deleted
    /// </summary>
    [Boolean]
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Internal score from Elasticsearch for query results
    /// </summary>
    [Ignore]
    public double? Score { get; set; }
}