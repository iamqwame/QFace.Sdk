using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.ActorSystems;
using Demo.ActorSystems.Api.Actors;

namespace Demo.ActorSystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IActorService _actorService;
    private readonly ILogger<TestController> _logger;

    public TestController(IActorService actorService, ILogger<TestController> logger)
    {
        _actorService = actorService;
        _logger = logger;
    }

    /// <summary>
    /// Send a test message to the TestActor
    /// </summary>
    [HttpPost("test-actor")]
    public IActionResult SendToTestActor([FromBody] TestMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending message to TestActor: {Message}", request.Message);
            
            // Send message to the test actor
            _actorService.Tell<TestActor>(request.Message);
            
            return Ok(new { 
                message = "Message sent to TestActor successfully",
                sentMessage = request.Message,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to TestActor");
            return StatusCode(500, new { error = "Failed to send message to TestActor" });
        }
    }

    /// <summary>
    /// Send a user message to the UserActor
    /// </summary>
    [HttpPost("user-actor")]
    public IActionResult SendToUserActor([FromBody] UserMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending user message to UserActor: {Name} ({Email})", request.Name, request.Email);
            
            // Send user message to the user actor
            _actorService.Tell<UserActor>(new UserMessage(request.Name, request.Email));
            
            return Ok(new { 
                message = "User message sent to UserActor successfully",
                userName = request.Name,
                userEmail = request.Email,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send user message to UserActor");
            return StatusCode(500, new { error = "Failed to send user message to UserActor" });
        }
    }

    /// <summary>
    /// Send a string message to the MultiPurposeActor
    /// </summary>
    [HttpPost("multi-purpose/string")]
    public IActionResult SendStringToMultiPurpose([FromBody] TestMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending string to MultiPurposeActor: {Message}", request.Message);
            
            _actorService.Tell<MultiPurposeActor>(request.Message);
            
            return Ok(new { 
                message = "String sent to MultiPurposeActor successfully",
                sentMessage = request.Message,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send string to MultiPurposeActor");
            return StatusCode(500, new { error = "Failed to send string to MultiPurposeActor" });
        }
    }

    /// <summary>
    /// Send a user message to the MultiPurposeActor
    /// </summary>
    [HttpPost("multi-purpose/user")]
    public IActionResult SendUserToMultiPurpose([FromBody] UserMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending user to MultiPurposeActor: {Name} ({Email})", request.Name, request.Email);
            
            _actorService.Tell<MultiPurposeActor>(new UserMessage(request.Name, request.Email));
            
            return Ok(new { 
                message = "User sent to MultiPurposeActor successfully",
                userName = request.Name,
                userEmail = request.Email,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send user to MultiPurposeActor");
            return StatusCode(500, new { error = "Failed to send user to MultiPurposeActor" });
        }
    }

    /// <summary>
    /// Send an email message to the MultiPurposeActor
    /// </summary>
    [HttpPost("multi-purpose/email")]
    public IActionResult SendEmailToMultiPurpose([FromBody] EmailMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email to MultiPurposeActor: {Subject} to {ToEmail}", request.Subject, request.ToEmail);
            
            _actorService.Tell<MultiPurposeActor>(new EmailMessage(request.ToEmail, request.Subject, request.Body));
            
            return Ok(new { 
                message = "Email sent to MultiPurposeActor successfully",
                toEmail = request.ToEmail,
                subject = request.Subject,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to MultiPurposeActor");
            return StatusCode(500, new { error = "Failed to send email to MultiPurposeActor" });
        }
    }

    /// <summary>
    /// Send a number to the MultiPurposeActor
    /// </summary>
    [HttpPost("multi-purpose/number")]
    public IActionResult SendNumberToMultiPurpose([FromBody] NumberMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending number to MultiPurposeActor: {Number}", request.Number);
            
            _actorService.Tell<MultiPurposeActor>(request.Number);
            
            return Ok(new { 
                message = "Number sent to MultiPurposeActor successfully",
                number = request.Number,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send number to MultiPurposeActor");
            return StatusCode(500, new { error = "Failed to send number to MultiPurposeActor" });
        }
    }

    /// <summary>
    /// Send a command to the MultiPurposeActor
    /// </summary>
    [HttpPost("multi-purpose/command")]
    public IActionResult SendCommandToMultiPurpose([FromBody] CommandMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Sending command to MultiPurposeActor: {Command} with data: {Data}", request.Command, request.Data);
            
            _actorService.Tell<MultiPurposeActor>(new CommandMessage(request.Command, request.Data));
            
            return Ok(new { 
                message = "Command sent to MultiPurposeActor successfully",
                command = request.Command,
                data = request.Data,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command to MultiPurposeActor");
            return StatusCode(500, new { error = "Failed to send command to MultiPurposeActor" });
        }
    }

    /// <summary>
    /// Get actor system status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        try
        {
            _logger.LogInformation("Getting actor system status");
            
            return Ok(new { 
                status = "running",
                actors = new[] { "TestActor", "UserActor", "MultiPurposeActor" },
                multiPurposeActorHandles = new[] { "string", "UserMessage", "EmailMessage", "int", "CommandMessage" },
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get actor system status");
            return StatusCode(500, new { error = "Failed to get actor system status" });
        }
    }
}

// Request DTOs
public class TestMessageRequest
{
    public string Message { get; set; } = string.Empty;
}

public class UserMessageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EmailMessageRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class NumberMessageRequest
{
    public int Number { get; set; }
}

public class CommandMessageRequest
{
    public string Command { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
