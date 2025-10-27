using QFace.Sdk.ActorSystems;

namespace Demo.ActorSystems.Api.Actors;

/// <summary>
/// Simple test actor for handling string messages
/// </summary>
public class TestActor : BaseActor
{
    private readonly ILogger<TestActor> _logger;

    public TestActor(ILogger<TestActor> logger)
    {
        _logger = logger;
        
        // Set up message handling
        Receive<string>(message =>
        {
            _logger.LogInformation("🎭 [TestActor] Received message: {Message}", message);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] TestActor received: {message}");
        });
    }
}

/// <summary>
/// Simple actor for handling user messages
/// </summary>
public class UserActor : BaseActor
{
    private readonly ILogger<UserActor> _logger;

    public UserActor(ILogger<UserActor> logger)
    {
        _logger = logger;
        
        // Set up message handling
        Receive<UserMessage>(message =>
        {
            _logger.LogInformation("👤 [UserActor] Processing user: {Name} ({Email})", 
                message.Name, message.Email);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] UserActor processed: {message.Name} - {message.Email}");
        });
    }
}

/// <summary>
/// Multi-purpose actor that handles multiple message types
/// </summary>
public class MultiPurposeActor : BaseActor
{
    private readonly ILogger<MultiPurposeActor> _logger;

    public MultiPurposeActor(ILogger<MultiPurposeActor> logger)
    {
        _logger = logger;
        
        // Multiple Receive handlers for different message types
        
        // Handle string messages
        Receive<string>(message =>
        {
            _logger.LogInformation("🔤 [MultiPurposeActor] String message: {Message}", message);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MultiPurposeActor (String): {message}");
        });
        
        // Handle user messages
        Receive<UserMessage>(message =>
        {
            _logger.LogInformation("👤 [MultiPurposeActor] User message: {Name} ({Email})", 
                message.Name, message.Email);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MultiPurposeActor (User): {message.Name} - {message.Email}");
        });
        
        // Handle email messages
        Receive<EmailMessage>(message =>
        {
            _logger.LogInformation("📧 [MultiPurposeActor] Email message: {Subject} to {ToEmail}", 
                message.Subject, message.ToEmail);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MultiPurposeActor (Email): {message.Subject} to {message.ToEmail}");
        });
        
        // Handle number messages
        Receive<int>(number =>
        {
            _logger.LogInformation("🔢 [MultiPurposeActor] Number message: {Number}", number);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MultiPurposeActor (Number): {number}");
        });
        
        // Handle custom command messages
        Receive<CommandMessage>(command =>
        {
            _logger.LogInformation("⚡ [MultiPurposeActor] Command: {Command} with data: {Data}", 
                command.Command, command.Data);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MultiPurposeActor (Command): {command.Command} - {command.Data}");
        });
    }
}

// Simple message types for testing
public record UserMessage(string Name, string Email);
public record EmailMessage(string ToEmail, string Subject, string Body);
public record CommandMessage(string Command, string Data);
