using FluentAssertions;
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
}
