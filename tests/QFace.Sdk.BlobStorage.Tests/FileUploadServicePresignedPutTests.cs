using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using QFace.Sdk.BlobStorage.Models;
using QFace.Sdk.BlobStorage.Services;
using Xunit;

namespace QFace.Sdk.BlobStorage.Tests;

public class FileUploadServicePresignedPutTests
{
    [Fact]
    public async Task CreatePresignedPutUrlAsync_Signs_Put_With_ContentType_And_ContentLength()
    {
        GetPreSignedUrlRequest? captured = null;
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => captured = r)
            .Returns("https://example.r2.cloudflarestorage.com/bucket/learning/covers/a.png?X-Amz-Signature=test");

        var options = Options.Create(new BlobStorageOptions
        {
            Provider = "CloudflareR2",
            Region = "auto",
            ServiceURL = "https://example.r2.cloudflarestorage.com",
            Bucket = new BlobStorageBucketOptions
            {
                Name = "qimerp-bucket",
                CdnBaseDomain = "pub-98e94b52ba1548f4819a081bd36069ca.r2.dev"
            }
        });

        var service = new FileUploadService(
            s3.Object,
            options,
            NullLogger<FileUploadService>.Instance,
            Mock.Of<IImageResizeService>());

        var key = "learning/tenant/courses/covers/abc--cover.png";
        var result = await service.CreatePresignedPutUrlAsync(key, "image/png", 2048, expirationMinutes: 10);

        captured.Should().NotBeNull();
        captured!.Verb.Should().Be(HttpVerb.PUT);
        captured.ContentType.Should().Be("image/png");
        captured.Headers.ContentLength.Should().Be(2048);
        captured.Key.Should().Be(key);
        captured.BucketName.Should().Be("qimerp-bucket");

        result.Key.Should().Be(key);
        result.PutUrl.Should().Contain("X-Amz-Signature=test");
        result.PublicUrl.Should().Be(
            "https://pub-98e94b52ba1548f4819a081bd36069ca.r2.dev/learning/tenant/courses/covers/abc--cover.png");
    }
}
