namespace QFace.Sdk.RedisMq.Consumer;

public class ConsumerMetadata
{
    public Type ConsumerType { get; set; }
    public MethodInfo HandlerMethod { get; set; }
    public ChannelAttribute ChannelAttribute { get; set; }
}
