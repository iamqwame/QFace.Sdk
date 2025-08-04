namespace QFace.Sdk.Kafka.Consumer;

/// <summary>
/// Metadata about a Kafka consumer method
/// </summary>
public class ConsumerMetadata
{
    public Type ConsumerType { get; set; }
    public MethodInfo HandlerMethod { get; set; }
    public ConsumeTopicAttribute TopicAttribute { get; set; }
    public List<string> Topics { get; set; } = new();
    public string TopicGroup { get; set; }
    public ProcessingConfig ProcessingConfig { get; set; }
    public string ConsumerGroupId { get; set; }
}

/// <summary>
/// Processing configuration for a specific consumer method
/// </summary>
public class ProcessingConfig
{
    public int MaxBatchSize { get; set; }
    public int BatchTimeoutMs { get; set; }
    public bool IsBulk { get; set; }
    public OffsetCommitStrategy CommitStrategy { get; set; }
    public string DeadLetterTopic { get; set; }
}

/// <summary>
/// Utility class for resolving topics from configuration
/// </summary>
public static class TopicResolver
{
    /// <summary>
    /// Resolve topics for a consumer method based on its attribute configuration
    /// </summary>
    public static List<string> ResolveTopics(ConsumeTopicAttribute attribute, IOptions<KafkaConsumerConfig> config)
    {
        // Priority 1: Direct topics specified in attribute
        if (attribute.DirectTopics?.Any() == true)
        {
            return attribute.DirectTopics.ToList();
        }
        
        // Priority 2: Named topic group
        if (!string.IsNullOrEmpty(attribute.TopicGroup))
        {
            if (config.Value.TopicGroups.TryGetValue(attribute.TopicGroup, out var groupTopics))
            {
                if (!groupTopics.Any())
                {
                    throw new InvalidOperationException(
                        $"Topic group '{attribute.TopicGroup}' exists but contains no topics");
                }
                return groupTopics;
            }
            
            throw new InvalidOperationException(
                $"Topic group '{attribute.TopicGroup}' not found in configuration. " +
                $"Available groups: {string.Join(", ", config.Value.TopicGroups.Keys)}");
        }
        
        // Priority 3: Custom config path
        if (!string.IsNullOrEmpty(attribute.ConfigPath))
        {
            // This would require IConfiguration injection - implement if needed
            throw new NotImplementedException("Custom config path resolution not yet implemented");
        }
        
        // Fallback: Use general topics list
        if (config.Value.Topics.Any())
        {
            return config.Value.Topics;
        }
        
        throw new InvalidOperationException(
            "No topics found. Specify either DirectTopics, TopicGroup, or configure general Topics list");
    }
    
    /// <summary>
    /// Validate that all required topic groups exist in configuration
    /// </summary>
    public static void ValidateTopicGroups(IEnumerable<ConsumerMetadata> consumers, KafkaConsumerConfig config)
    {
        var requiredGroups = consumers
            .Where(c => !string.IsNullOrEmpty(c.TopicGroup))
            .Select(c => c.TopicGroup)
            .Distinct();
            
        var missingGroups = requiredGroups
            .Where(group => !config.TopicGroups.ContainsKey(group))
            .ToList();
            
        if (missingGroups.Any())
        {
            throw new InvalidOperationException(
                $"Missing topic groups in configuration: {string.Join(", ", missingGroups)}");
        }
    }
}
