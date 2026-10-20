using FluentAssertions;
using QimErp.Shared.Common.Security;
using Xunit;

namespace QimErp.Shared.Common.Tests.Security;

public class BlindIndexTests
{
    private static readonly byte[] Key = "unit-test-blind-index-key-32b!!"u8.ToArray();

    [Fact]
    public void Compute_is_deterministic_and_case_insensitive()
    {
        var index = new BlindIndex(Key);

        var a = index.Compute("  Ada@Example.COM ");
        var b = index.Compute("ada@example.com");

        a.Should().Be(b);
        a.Should().HaveLength(64);
    }

    [Fact]
    public void Compute_differs_for_different_values()
    {
        var index = new BlindIndex(Key);

        index.Compute("one@example.com").Should().NotBe(index.Compute("two@example.com"));
    }

    [Fact]
    public void Compute_rejects_null()
    {
        var index = new BlindIndex(Key);
        var act = () => index.Compute(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
