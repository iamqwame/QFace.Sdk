namespace QFace.Sdk.RedisMq.Models;

public class RedisMqOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string Title { get; set; }
    public int RetryCount { get; set; } = 5;
    public int RetryIntervalMs { get; set; } = 1000;
    public int PublisherInstances { get; set; } = 10;
    public int PublisherUpperBound { get; set; } = 100;
    public int ConsumerInstances { get; set; } = 10;
    public int ConsumerUpperBound { get; set; } = 100;
    public int Database { get; set; } = 0;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;
}
