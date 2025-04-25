using Microsoft.Extensions.Logging;

namespace Demo.RabbitMq.Consumer;

[Consumer("DemoMessageConsumer")]
public class MessageConsumer
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    [Topic("demo_exchange", "important_messages")]
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

    [Topic("demo_exchange",  "normal_messages")]
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

    // This will receive all messages with routing key starting with "message."
    [Topic("demo_exchange",  "all_messages")]
    public async Task HandleAllMessages(MessageDto message)
    {
        _logger.LogInformation(">>> ALL MESSAGES LISTENER <<<");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation(">>>>>>>>>>>>>>><<<<<<<<<<<<<<<");    await Task.Delay(100);
    }
}

public class MessageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}