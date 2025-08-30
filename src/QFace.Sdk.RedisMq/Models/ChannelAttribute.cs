namespace QFace.Sdk.RedisMq.Models;

[AttributeUsage(AttributeTargets.Method)]
public class ChannelAttribute : Attribute
{
    public string ChannelName { get; set; }
    
    public ChannelAttribute(string channelName)
    {
        ChannelName = channelName;
    }
}
