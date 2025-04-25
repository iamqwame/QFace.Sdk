using QFace.Sdk.RabbitMq.Models;

namespace QFace.Sdk.RabbitMq.Consumer;

public class ConsumerMetadata
{
    public Type ConsumerType { get; set; }
    public MethodInfo HandlerMethod { get; set; }
    public TopicAttribute TopicAttribute { get; set; }
}