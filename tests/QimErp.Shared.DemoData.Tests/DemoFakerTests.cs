using FluentAssertions;
using QimErp.Shared.DemoData.Bogus;
using Xunit;

namespace QimErp.Shared.DemoData.Tests;

public class DemoFakerTests
{
    [Fact]
    public void SameSeed_ProducesSameOutput()
    {
        var a = new DemoFaker(42);
        var b = new DemoFaker(42);

        var fromA = Enumerable.Range(0, 10).Select(_ => a.Faker.Random.Int(1, 1000)).ToArray();
        var fromB = Enumerable.Range(0, 10).Select(_ => b.Faker.Random.Int(1, 1000)).ToArray();

        fromA.Should().Equal(fromB);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentOutput()
    {
        var a = new DemoFaker(42);
        var b = new DemoFaker(43);

        var fromA = Enumerable.Range(0, 10).Select(_ => a.Faker.Random.Int(1, 1000)).ToArray();
        var fromB = Enumerable.Range(0, 10).Select(_ => b.Faker.Random.Int(1, 1000)).ToArray();

        var differingPositions = fromA.Zip(fromB, (x, y) => x != y).Count(diff => diff);
        differingPositions.Should().BeGreaterThanOrEqualTo(5);
    }
}
