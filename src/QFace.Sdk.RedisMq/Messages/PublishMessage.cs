namespace QFace.Sdk.RedisMq.Messages;

internal class PublishMessage
{
    public object Message { get; }
    public string ChannelName { get; }
    public Type MessageType { get; }

    public PublishMessage(object message, string channelName)
    {
        Message = message;
        ChannelName = channelName;
        MessageType = message.GetType();
    }
}
