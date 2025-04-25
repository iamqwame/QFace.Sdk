namespace QFace.Sdk.RabbitMq.Services;

public interface IRabbitMqPublisher
{
    Task<bool> PublishAsync<T>(T message, string exchangeName,string routingKey="");
}

