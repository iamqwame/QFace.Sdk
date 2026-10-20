using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Security;

public sealed class PiiFieldProtectorOptions
{
    public const string SectionName = "PiiFieldProtector";

    public string? KeyBase64 { get; set; }
}

public static class PiiFieldProtectorServiceCollectionExtensions
{
    // DATA-25: register AES-GCM protector for PII/Sensitive columns at rest.
    public static IServiceCollection AddPiiFieldProtector(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PiiFieldProtectorOptions>(
            configuration.GetSection(PiiFieldProtectorOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<PiiFieldProtectorOptions>>().Value;
            var env = sp.GetRequiredService<IHostEnvironment>();
            var config = sp.GetRequiredService<IConfiguration>();
            return new PiiFieldProtector(ResolveKeyBytes(opts.KeyBase64, config, env));
        });
        return services;
    }

    private static byte[] ResolveKeyBytes(
        string? keyBase64,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        if (!string.IsNullOrWhiteSpace(keyBase64))
        {
            var key = Convert.FromBase64String(keyBase64);
            if (key.Length is not (16 or 24 or 32))
                throw new InvalidOperationException(
                    "PiiFieldProtector:KeyBase64 must be a base64-encoded AES-128/192/256 key.");
            return key;
        }

        var jwtSecret = configuration["Jwt:Secret"];
        if (!string.IsNullOrWhiteSpace(jwtSecret))
            return SHA256.HashData(Encoding.UTF8.GetBytes("pii-field:" + jwtSecret));

        if (env.IsProduction())
        {
            throw new InvalidOperationException(
                "PiiFieldProtector:KeyBase64 or Jwt:Secret is required in Production (DATA-25).");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes("qimerp-dev-pii-field-key"));
    }
}
