namespace QFace.Sdk.Elasticsearch.Models;

/// <summary>
/// Attribute to specify the Elasticsearch type name for a document
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ElasticsearchTypeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the Elasticsearch type/index
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates a new instance of the ElasticsearchTypeAttribute
    /// </summary>
    /// <param name="name">The name of the Elasticsearch type/index</param>
    public ElasticsearchTypeAttribute(string name)
    {
        Name = name;
    }
}
