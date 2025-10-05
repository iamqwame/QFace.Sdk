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
    [Topic(
        exchangeName: "umat.core.admissions_payment_made.prod_exchange",
        queueName:    "umat.admissions.payment.queue"
    )]
    public async Task HandleImportantMessage(PublishRequest message)
    {
        _logger.LogInformation("=== IMPORTANT MESSAGE RECEIVED ===");
        _logger.LogInformation($"ID: {message.PaymentItemId}");
        _logger.LogInformation($"Content: {message.Amount}");
        _logger.LogInformation($"Timestamp: {message.TransactionDate}");
        _logger.LogInformation("=================================");
        
        // Simulate processing time
        await Task.Delay(500);
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


public class PublishRequest
{
    public string TransactionId { get; set; }
    public string PaymentItemId { get; set; }
    public string PaymentItemName { get; set; }
    public string PaymentItemDescription { get; set; }
    public int OriginalItemId { get; set; }
    public string VendorId { get; set; }
    public string VendorUsername { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string ReferenceNumber { get; set; }
    public string TellerId { get; set; }
    public string TellerName { get; set; }
    public string BankBranch { get; set; }
    public string Notes { get; set; }
    public string FormReferenceId { get; set; }
    public string FormPinCode { get; set; }
    public string Category { get; set; }
    public string BankTransactionId { get; set; }
    public string? ReferenceApplicationTypeId { get; set; }
    public string PaymentItemCategory { get; set; }
    public List<string> PaymentItemTags { get; set; } = [];
    public string? IpAddress { get; set; }
    public bool IsReversalRequested { get; set; }
    public string? ReversalTellerId { get; set; }
    public string? ReversalTellerName { get; set; }
    public string? ReversalReason { get; set; }
    public string? ReversalIP { get; set; }
    public bool IsReversalApproved { get; set; }
    public string? ReversalApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }
}