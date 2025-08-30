namespace QFace.Sdk.RedisMq.Messages;

internal class ConsumeMessage
{
    public string ChannelName { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    public ConsumeMessage(string channelName, string message)
    {
        ChannelName = channelName;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}
