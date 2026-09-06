using FluentAssertions;
using QFace.Sdk.BlobStorage.Models;
using QFace.Sdk.BlobStorage.Services;
using Xunit;

namespace QFace.Sdk.BlobStorage.Tests;

public class BlobCdnUrlTests
{
    [Fact]
    public void CloudflareR2_PublicHost_Is_Path_Style()
    {
        var options = R2("pub-98e94b52ba1548f4819a081bd36069ca.r2.dev");

        var url = BlobCdnUrl.FromKey(options, "learning/covers/safety.png");

        url.Should().Be("https://pub-98e94b52ba1548f4819a081bd36069ca.r2.dev/learning/covers/safety.png");
    }

    [Fact]
    public void CloudflareR2_Strips_Scheme_And_Trailing_Slash()
    {
        var options = R2("https://pub-98e94b52ba1548f4819a081bd36069ca.r2.dev/");

        BlobCdnUrl.FromKey(options, "a/b.png")
            .Should().Be("https://pub-98e94b52ba1548f4819a081bd36069ca.r2.dev/a/b.png");
    }

    [Fact]
    public void CloudflareR2_Custom_Domain_Is_Path_Style()
    {
        var options = R2("media.qimerp.com");

        BlobCdnUrl.FromKey(options, "posters/1.jpg")
            .Should().Be("https://media.qimerp.com/posters/1.jpg");
    }

    [Fact]
    public void Empty_Cdn_Uses_Private_R2_Service_Url()
    {
        var options = R2("");
        options.ServiceURL = "https://081a908391f2d33817773c1539ef7c93.r2.cloudflarestorage.com";

        BlobCdnUrl.FromKey(options, "learning/covers/safety.png")
            .Should().Be("https://081a908391f2d33817773c1539ef7c93.r2.cloudflarestorage.com/qimerp-bucket/learning/covers/safety.png");
    }

    [Fact]
    public void DigitalOcean_Keeps_Bucket_Region_Host()
    {
        var options = new BlobStorageOptions
        {
            Provider = "DigitalOcean",
            Region = "nyc3",
            Bucket = new BlobStorageBucketOptions
            {
                Name = "qimerp-bucket",
                CdnBaseDomain = "cdn.digitaloceanspaces.com"
            }
        };

        BlobCdnUrl.FromKey(options, "avatars/a.png")
            .Should().Be("https://qimerp-bucket.nyc3.cdn.digitaloceanspaces.com/avatars/a.png");
    }

    private static BlobStorageOptions R2(string cdn) => new()
    {
        Provider = "CloudflareR2",
        Region = "us-east-1",
        Bucket = new BlobStorageBucketOptions
        {
            Name = "qimerp-bucket",
            CdnBaseDomain = cdn
        }
    };
}
