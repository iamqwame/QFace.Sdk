using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using QimErp.Shared.Common.Security;
using Xunit;

namespace QimErp.Shared.Common.Tests.Security;

public class PiiFieldProtectorTests
{
    [Fact]
    public void Protect_then_Unprotect_round_trips()
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes("qimerp-dev-pii-field-key"));
        var protector = new PiiFieldProtector(key);

        var cipher = protector.Protect("ama@example.com");
        cipher.Should().NotBe("ama@example.com");
        protector.Unprotect(cipher).Should().Be("ama@example.com");
    }

    [Fact]
    public void Protect_is_non_deterministic()
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes("qimerp-dev-pii-field-key"));
        var protector = new PiiFieldProtector(key);

        var a = protector.Protect("same");
        var b = protector.Protect("same");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unprotect_rejects_truncated_payload()
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes("qimerp-dev-pii-field-key"));
        var protector = new PiiFieldProtector(key);

        var act = () => protector.Unprotect(Convert.ToBase64String(new byte[8]));
        act.Should().Throw<CryptographicException>();
    }
}
