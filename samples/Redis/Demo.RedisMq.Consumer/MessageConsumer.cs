using Microsoft.Extensions.Logging;

namespace Demo.RedisMq.Consumer;

[Consumer("DemoMessageConsumer")]
public class MessageConsumer
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    [Channel("important_messages")]
    public async Task HandleImportantMessage(MessageDto message)
    {
        _logger.LogInformation("=== IMPORTANT MESSAGE RECEIVED ===");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation("=================================");
        
        // Simulate processing time
        await Task.Delay(500);
    }

    [Channel("normal_messages")]
    public async Task HandleNormalMessage(MessageDto message)
    {
        _logger.LogInformation("--- NORMAL MESSAGE RECEIVED ---");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation("-----------------------------");
        
        // Simulate processing time
        await Task.Delay(300);
    }

    [Channel("demo_channel")]
    public async Task HandleDemoMessage(MessageDto message)
    {
        _logger.LogInformation(">>> DEMO CHANNEL MESSAGE <<<");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation(">>>>>>>>>>>>>>><<<<<<<<<<<<<<<");
        
        await Task.Delay(100);
    }
}

public class MessageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
