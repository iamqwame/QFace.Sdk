namespace QFace.Sdk.RedisMq.Services;

public interface IRedisMqPublisher
{
    Task<bool> PublishAsync<T>(T message, string channelName);
}
