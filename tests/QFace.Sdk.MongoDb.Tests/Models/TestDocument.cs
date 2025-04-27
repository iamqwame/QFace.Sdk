using QFace.Sdk.MongoDb.Models;

namespace QFace.Sdk.MongoDb.Tests.Models;

/// <summary>
/// Test document for MongoDB integration tests
/// </summary>
public class TestDocument : BaseDocument
{
    /// <summary>
    /// Name of the test document
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Description of the test document
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Score of the test document (for testing numeric values)
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Tags for the test document (for testing arrays)
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Metadata for the test document (for testing nested objects)
    /// </summary>
    public TestMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Test metadata for nested document testing
/// </summary>
public class TestMetadata
{
    /// <summary>
    /// Version of the test metadata
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// Created date of the test metadata
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Properties for the test metadata (for testing dictionaries)
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}