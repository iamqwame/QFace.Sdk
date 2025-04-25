namespace QFace.Sdk.RabbitMq.Messages;

internal class PublishMessage
{
    public object Message { get; }
    public string RoutingKey { get; }
    public string ExchangeName { get; }
    public Type MessageType { get; }

    public PublishMessage(object message, string routingKey,string exchangeName)
    {
        Message = message;
        RoutingKey = routingKey;
        ExchangeName = exchangeName;
        MessageType = message.GetType();
    }
}