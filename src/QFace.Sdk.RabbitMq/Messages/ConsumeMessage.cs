namespace QFace.Sdk.RabbitMq.Messages;

internal class ConsumeMessage
{
    public string[] ConsumerTag { get; }
    public string QueueName { get; }
    public BasicDeliverEventArgs DeliveryArgs { get; }

    public ConsumeMessage(string[] consumerTag, BasicDeliverEventArgs deliveryArgs, string queueName)
    {
        ConsumerTag = consumerTag;
        DeliveryArgs = deliveryArgs;
        QueueName = queueName;
    }
}