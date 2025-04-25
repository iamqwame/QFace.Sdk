namespace QFace.Sdk.RabbitMq.Models;

[AttributeUsage(AttributeTargets.Class)]
public class ConsumerAttribute : Attribute
{
    public string Name { get; set; }
        
    public ConsumerAttribute(string name = null)
    {
        Name = name;
    }
}