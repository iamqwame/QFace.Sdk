using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;

namespace QimErp.Shared.Common.Services.LegalHold;

public sealed class LegalHoldGateClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IOptions<InternalApiOptions> internalApiOptions,
    ILogger<LegalHoldGateClient> logger) : ILegalHoldGate
{
    public const string HttpClientName = nameof(LegalHoldGateClient);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<bool> HasActivePersonLegalHoldAsync(
        string tenantId,
        Guid personId,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || personId == Guid.Empty)
            return true;

        var baseUrl = configuration["WorkflowServiceUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            logger.LogError(
                "WorkflowServiceUrl is not configured — treating person {PersonId} as held",
                personId);
            return true;
        }

        var apiKey = internalApiOptions.Value.ExpectedApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogError(
                "InternalApi:ExpectedApiKey is not configured — treating person {PersonId} as held",
                personId);
            return true;
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

            var normalizedTenantId = NormalizeTenantId(tenantId);
            var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
            var path =
                $"internal/legal-holds/person/{personId:D}?tenantId={Uri.EscapeDataString(normalizedTenantId)}&asOfUtc={Uri.EscapeDataString(asOf.ToString("O"))}";

            var response = await client.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Legal hold lookup returned {StatusCode} for person {PersonId}; treating as held",
                    (int)response.StatusCode,
                    personId);
                return true;
            }

            LegalHoldGateResponse? body;
            try
            {
                body = await response.Content.ReadFromJsonAsync<LegalHoldGateResponse>(JsonOptions, cancellationToken);
            }
            catch (Exception parseEx) when (parseEx is not OperationCanceledException)
            {
                logger.LogWarning(
                    parseEx,
                    "Legal hold response parse failed for person {PersonId}; treating as held",
                    personId);
                return true;
            }

            if (body is null)
            {
                logger.LogWarning(
                    "Legal hold response was empty for person {PersonId}; treating as held",
                    personId);
                return true;
            }

            if (body.HasActiveHold is null)
            {
                logger.LogWarning(
                    "Legal hold response missing hasActiveHold for person {PersonId}; treating as held",
                    personId);
                return true;
            }

            return body.HasActiveHold.Value;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Legal hold lookup failed for person {PersonId}; treating as held",
                personId);
            return true;
        }
    }

    // Matches TenantContextMiddleware.NormalizeTenantId (internal); Guid claims may differ only by casing.
    private static string NormalizeTenantId(string tenantId) =>
        Guid.TryParse(tenantId.Trim(), out var g)
            ? g.ToString("D").ToLowerInvariant()
            : tenantId.Trim();
}
