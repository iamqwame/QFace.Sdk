using FluentAssertions;
using Microsoft.Extensions.Logging;
using QFace.Sdk.RedisCache.Models;
using QFace.Sdk.RedisCache.Services.Providers;
using StackExchange.Redis;
using Xunit;

namespace QFace.Sdk.RedisCache.Tests;

public class StackExchangeRedisProviderTests
{
    [Fact]
    public void DescribeEndpoints_OmitsPasswordAndUser()
    {
        var options = ConfigurationOptions.Parse("redis.example.com:6380,user=svc,password=s3cr3t-value,ssl=true");

        var described = StackExchangeRedisProvider.DescribeEndpoints(options);

        described.Should().Contain("redis.example.com:6380");
        described.Should().NotContain("s3cr3t-value");
        described.Should().NotContainEquivalentOf("password");
        described.Should().NotContain("svc");
    }

    [Fact]
    public void DescribeEndpoints_ListsEveryEndpoint()
    {
        var options = ConfigurationOptions.Parse("a.example.com:6379,b.example.com:6379,password=x");

        StackExchangeRedisProvider.DescribeEndpoints(options)
            .Should().Be("a.example.com:6379,b.example.com:6379");
    }

    [Fact]
    public void Constructor_NeverLogsTheConnectionStringCredentials()
    {
        var logger = new CapturingLogger();
        var options = new StackExchangeOptions
        {
            ConnectionString = "127.0.0.1:1,user=svc-user,password=s3cr3t-value",
            AbortOnConnectFail = false,
            ConnectTimeout = 100,
        };

        _ = new StackExchangeRedisProvider(options, logger);

        logger.Messages.Should().NotBeEmpty();
        logger.Messages.Should().AllSatisfy(m =>
        {
            m.Should().NotContain("s3cr3t-value");
            m.Should().NotContain("svc-user");
            m.Should().NotContainEquivalentOf("password");
        });
        logger.Messages.Should().Contain(m => m.Contains("127.0.0.1:1"));
    }

    private sealed class CapturingLogger : ILogger<StackExchangeRedisProvider>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception) + (exception is null ? "" : " " + exception));
    }
}
