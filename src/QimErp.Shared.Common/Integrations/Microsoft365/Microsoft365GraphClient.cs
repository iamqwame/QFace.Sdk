using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Integrations.Microsoft365;

public sealed class Microsoft365GraphClient(
    ICacheService cache,
    IHttpClientFactory httpClientFactory,
    ILogger<Microsoft365GraphClient> logger) : IMicrosoft365GraphClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task<TenantMicrosoft365ResolvedOptions?> GetTenantOptionsAsync(
        string tenantId,
        CancellationToken cancellationToken = default) =>
        cache.GetAsync<TenantMicrosoft365ResolvedOptions>(SharedCacheKeys.TenantMicrosoft365Config(tenantId));

    public async Task<Microsoft365OnlineMeetingResult> TryCreateOnlineMeetingAsync(
        string tenantId,
        string subject,
        DateTime startUtc,
        DateTime endUtc,
        string organizerUserPrincipalName,
        CancellationToken cancellationToken = default)
    {
        var options = await GetTenantOptionsAsync(tenantId, cancellationToken);
        if (options is null || !options.EnableTeamsMeetingOnCompanyEvents)
        {
            return new Microsoft365OnlineMeetingResult
            {
                Succeeded = false,
                Error = "Teams meetings for company events are not enabled."
            };
        }

        if (string.IsNullOrWhiteSpace(organizerUserPrincipalName))
        {
            return new Microsoft365OnlineMeetingResult
            {
                Succeeded = false,
                Error = "Organizer UPN is required to create a Teams meeting."
            };
        }

        try
        {
            var token = await AcquireTokenAsync(options, cancellationToken);
            if (token is null)
            {
                return new Microsoft365OnlineMeetingResult
                {
                    Succeeded = false,
                    Error = "Could not obtain a Graph token."
                };
            }

            using var client = httpClientFactory.CreateClient("microsoft365-graph");
            var body = new
            {
                startDateTime = startUtc.ToUniversalTime().ToString("o"),
                endDateTime = endUtc.ToUniversalTime().ToString("o"),
                subject
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(organizerUserPrincipalName)}/onlineMeetings")
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Graph onlineMeeting create failed for tenant {TenantId} with status {Status}",
                    tenantId, (int)response.StatusCode);
                return new Microsoft365OnlineMeetingResult
                {
                    Succeeded = false,
                    Error = $"Graph returned {(int)response.StatusCode}."
                };
            }

            using var doc = JsonDocument.Parse(payload);
            var joinUrl = doc.RootElement.TryGetProperty("joinWebUrl", out var join)
                ? join.GetString()
                : null;

            return new Microsoft365OnlineMeetingResult
            {
                Succeeded = !string.IsNullOrWhiteSpace(joinUrl),
                JoinUrl = joinUrl,
                Error = string.IsNullOrWhiteSpace(joinUrl) ? "Graph response missing joinWebUrl." : null
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Graph onlineMeeting create failed for tenant {TenantId}", tenantId);
            return new Microsoft365OnlineMeetingResult
            {
                Succeeded = false,
                Error = "Teams meeting creation failed."
            };
        }
    }

    public async Task<Microsoft365OofResult> TrySetAutomaticRepliesAsync(
        string tenantId,
        string mailboxUserPrincipalName,
        DateTime startUtc,
        DateTime endUtc,
        string externalMessage,
        string internalMessage,
        bool clear,
        CancellationToken cancellationToken = default)
    {
        var options = await GetTenantOptionsAsync(tenantId, cancellationToken);
        if (options is null || !options.EnableOutlookOofOnLeave)
        {
            return new Microsoft365OofResult { Succeeded = true, Skipped = true };
        }

        if (string.IsNullOrWhiteSpace(mailboxUserPrincipalName))
        {
            return new Microsoft365OofResult
            {
                Succeeded = false,
                Error = "Mailbox UPN is required for Outlook OOF."
            };
        }

        try
        {
            var token = await AcquireTokenAsync(options, cancellationToken);
            if (token is null)
            {
                return new Microsoft365OofResult
                {
                    Succeeded = false,
                    Error = "Could not obtain a Graph token."
                };
            }

            object automaticRepliesSetting = clear
                ? new { status = "disabled" }
                : new
                {
                    status = "scheduled",
                    externalAudience = "all",
                    scheduledStartDateTime = new
                    {
                        dateTime = startUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeZone = "UTC"
                    },
                    scheduledEndDateTime = new
                    {
                        dateTime = endUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeZone = "UTC"
                    },
                    internalReplyMessage = internalMessage,
                    externalReplyMessage = externalMessage
                };

            var patchBody = new { automaticRepliesSetting };
            using var client = httpClientFactory.CreateClient("microsoft365-graph");
            using var request = new HttpRequestMessage(
                HttpMethod.Patch,
                $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(mailboxUserPrincipalName)}/mailboxSettings")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(patchBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Graph OOF update failed for tenant {TenantId} with status {Status}",
                    tenantId, (int)response.StatusCode);
                return new Microsoft365OofResult
                {
                    Succeeded = false,
                    Error = $"Graph returned {(int)response.StatusCode}."
                };
            }

            return new Microsoft365OofResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Graph OOF update failed for tenant {TenantId}", tenantId);
            return new Microsoft365OofResult
            {
                Succeeded = false,
                Error = "Outlook OOF update failed."
            };
        }
    }

    private async Task<string?> AcquireTokenAsync(
        TenantMicrosoft365ResolvedOptions options,
        CancellationToken cancellationToken)
    {
        var scope = options.ConsentedScopes.FirstOrDefault() ?? "https://graph.microsoft.com/.default";
        using var tokenClient = httpClientFactory.CreateClient("microsoft365-token");
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret,
            ["scope"] = scope.Contains(".default", StringComparison.Ordinal)
                ? scope
                : "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });

        var tokenUrl =
            $"https://login.microsoftonline.com/{Uri.EscapeDataString(options.EntraTenantId)}/oauth2/v2.0/token";
        using var response = await tokenClient.PostAsync(tokenUrl, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return doc.RootElement.TryGetProperty("access_token", out var token)
            ? token.GetString()
            : null;
    }
}
