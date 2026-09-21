namespace QFace.Sdk.Kafka.Models;

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// Additional Kafka producer properties
    /// </summary>
    public Dictionary<string, string> ExtraProperties { get; set; } = new();

    /// <summary>
    /// Producer behavior settings
    /// </summary>
    public string Acks { get; set; } = "all";
    public int Retries { get; set; } = 3;
    public int BatchSize { get; set; } = 16384;
    public int LingerMs { get; set; } = 5;
    public string CompressionType { get; set; } = "None";

    /// <summary>
    /// Actor pool settings
    /// </summary>
    public int ProducerInstances { get; set; } = 10;
    public int ProducerUpperBound { get; set; } = 100;

    /// <summary>
    /// Auto topic creation settings
    /// </summary>
    public AutoCreateTopicConfig AutoCreateTopics { get; set; } = new();
}

/// <summary>
/// Auto topic creation configuration
/// </summary>
public class AutoCreateTopicConfig
{
    public bool Enabled { get; set; } = true;
    public int NumberOfPartitions { get; set; } = 3;
    public short ReplicationFactor { get; set; } = 1;
}
