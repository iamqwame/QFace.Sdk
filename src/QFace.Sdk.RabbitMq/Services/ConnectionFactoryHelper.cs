using RabbitMQ.Client;
using QFace.Sdk.RabbitMq.Models;

namespace QFace.Sdk.RabbitMq.Services;

public static class ConnectionFactoryHelper
{
    public static ConnectionFactory CreateConnectionFactory(RabbitMqOptions options)
    {
        return new ConnectionFactory
        {
            Uri = new Uri(options.ConnectionString),
            AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(options.RequestedConnectionTimeout),
            RequestedHeartbeat = TimeSpan.FromSeconds(options.RequestedHeartbeat),
            ContinuationTimeout = TimeSpan.FromMilliseconds(options.ContinuationTimeout),
            SocketReadTimeout = TimeSpan.FromMilliseconds(options.SocketReadTimeout)
        };
    }
    
    /// <summary>
    /// Calculates exponential backoff delay: baseDelay * 2^attempt
    /// </summary>
    public static int CalculateExponentialBackoff(int baseDelayMs, int attempt, int maxDelayMs = 60000)
    {
        var delay = (int)(baseDelayMs * Math.Pow(2, attempt));
        return Math.Min(delay, maxDelayMs);
    }
}

