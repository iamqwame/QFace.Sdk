using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Options;

namespace QimErp.Shared.Common.Services.Retention;

public sealed class RetentionPolicyClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IOptions<InternalApiOptions> internalApiOptions,
    ILogger<RetentionPolicyClient> logger) : IRetentionPolicyClient
{
    public const string HttpClientName = nameof(RetentionPolicyClient);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<int?> GetRetentionDaysAfterCloseAsync(
        string tenantId,
        RetentionClass retentionClass,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var baseUrl = configuration["WorkflowServiceUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            logger.LogError(
                "WorkflowServiceUrl is not configured — skipping retention lookup for class {RetentionClass}",
                retentionClass);
            return null;
        }

        var apiKey = internalApiOptions.Value.ExpectedApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogError(
                "InternalApi:ExpectedApiKey is not configured — skipping retention lookup for class {RetentionClass}",
                retentionClass);
            return null;
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

            var normalizedTenantId = NormalizeTenantId(tenantId);
            var path =
                $"internal/retention-policies/{Uri.EscapeDataString(retentionClass.ToString())}?tenantId={Uri.EscapeDataString(normalizedTenantId)}";

            var response = await client.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Retention policy lookup returned {StatusCode} for class {RetentionClass}; treating as missing",
                    (int)response.StatusCode,
                    retentionClass);
                return null;
            }

            RetentionPolicyLookupResponse? body;
            try
            {
                body = await response.Content.ReadFromJsonAsync<RetentionPolicyLookupResponse>(JsonOptions, cancellationToken);
            }
            catch (Exception parseEx) when (parseEx is not OperationCanceledException)
            {
                logger.LogWarning(
                    parseEx,
                    "Retention policy response parse failed for class {RetentionClass}; treating as missing",
                    retentionClass);
                return null;
            }

            if (body is null)
            {
                logger.LogWarning(
                    "Retention policy response was empty for class {RetentionClass}; treating as missing",
                    retentionClass);
                return null;
            }

            return body.RetentionDaysAfterClose;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Retention policy lookup failed for class {RetentionClass}; treating as missing",
                retentionClass);
            return null;
        }
    }

    // Matches TenantContextMiddleware.NormalizeTenantId (internal); Guid claims may differ only by casing.
    private static string NormalizeTenantId(string tenantId) =>
        Guid.TryParse(tenantId.Trim(), out var g)
            ? g.ToString("D").ToLowerInvariant()
            : tenantId.Trim();
}
