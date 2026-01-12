using Microsoft.Extensions.Logging;

namespace Demo.RabbitMq.Consumer;

/// <summary>
/// Load test consumer with many handlers to verify connection sharing.
/// This simulates a real-world scenario with 15+ handlers.
/// Expected: All handlers should share 1 connection (not 15 connections)
/// </summary>
[Consumer("LoadTestConsumer")]
public class LoadTestConsumer
{
    private readonly ILogger<LoadTestConsumer> _logger;

    public LoadTestConsumer(ILogger<LoadTestConsumer> logger)
    {
        _logger = logger;
    }

    [Topic("load_test_exchange", "handler_1")]
    public async Task HandleMessage1(MessageDto message)
    {
        _logger.LogInformation("[Handler 1] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_2")]
    public async Task HandleMessage2(MessageDto message)
    {
        _logger.LogInformation("[Handler 2] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_3")]
    public async Task HandleMessage3(MessageDto message)
    {
        _logger.LogInformation("[Handler 3] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_4")]
    public async Task HandleMessage4(MessageDto message)
    {
        _logger.LogInformation("[Handler 4] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_5")]
    public async Task HandleMessage5(MessageDto message)
    {
        _logger.LogInformation("[Handler 5] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_6")]
    public async Task HandleMessage6(MessageDto message)
    {
        _logger.LogInformation("[Handler 6] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_7")]
    public async Task HandleMessage7(MessageDto message)
    {
        _logger.LogInformation("[Handler 7] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_8")]
    public async Task HandleMessage8(MessageDto message)
    {
        _logger.LogInformation("[Handler 8] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_9")]
    public async Task HandleMessage9(MessageDto message)
    {
        _logger.LogInformation("[Handler 9] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_10")]
    public async Task HandleMessage10(MessageDto message)
    {
        _logger.LogInformation("[Handler 10] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_11")]
    public async Task HandleMessage11(MessageDto message)
    {
        _logger.LogInformation("[Handler 11] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_12")]
    public async Task HandleMessage12(MessageDto message)
    {
        _logger.LogInformation("[Handler 12] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_13")]
    public async Task HandleMessage13(MessageDto message)
    {
        _logger.LogInformation("[Handler 13] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_14")]
    public async Task HandleMessage14(MessageDto message)
    {
        _logger.LogInformation("[Handler 14] Received: {Id}", message.Id);
        await Task.Delay(100);
    }

    [Topic("load_test_exchange", "handler_15")]
    public async Task HandleMessage15(MessageDto message)
    {
        _logger.LogInformation("[Handler 15] Received: {Id}", message.Id);
        await Task.Delay(100);
    }
}
