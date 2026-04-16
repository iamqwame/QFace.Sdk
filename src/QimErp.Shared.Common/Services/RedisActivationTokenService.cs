using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services;

public class ActivationTokenData
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Email data for resending
    public string Subject { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public Dictionary<string, string> Replacements { get; set; } = new();
    public List<string> EmailRecipients { get; set; } = new();
}

public class ActivationTokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public Dictionary<string, string> Replacements { get; set; } = new();
    public List<string> EmailRecipients { get; set; } = [];
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Token { get; set; }
}


public interface IRedisActivationTokenService
{
    Task<string> GenerateActivationTokenWithEmailDataAsync(ActivationTokenRequest request);
    Task<ActivationTokenData?> GetActivationTokenAsync(string email);
    Task<ActivationTokenData?> GetActivationTokenDataAsync(string email);
    Task<bool> ValidateAndConsumeActivationTokenAsync(string email, string token);
    Task<bool> HasActiveActivationTokenAsync(string email);
    Task RemoveActivationTokenAsync(string email);
}

public class RedisActivationTokenService(
    IDistributedCacheService cacheService,
    ILogger<RedisActivationTokenService> logger) : IRedisActivationTokenService
{
    private const int ActivationTokenTtlMinutes = 2880; // 48 hours
    private static string ActivationTokenCacheKey(string email) => $"qface:qimerp:auth:activation_token_{email}";

    public async Task<string> GenerateActivationTokenWithEmailDataAsync(ActivationTokenRequest request)
    {
        try
        {
            logger.LogInformation("🔑 [Redis Activation Token] Generating activation token with email data for {Email}", request.Email);

            // Generate a secure random token
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(ActivationTokenTtlMinutes);

            // Create activation token data
            var activationData = new ActivationTokenData
            {
                Token = request.Token,
                Email = request.Email,
                UserId = request.UserId,
                TenantId = request.TenantId,
                CreatedAt = now,
                ExpiresAt = expiresAt,
                IsUsed = false,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Subject = request.Subject,
                Template = request.Template,
                Replacements = request.Replacements,
                EmailRecipients = request.EmailRecipients
            };

            // Store in Redis with TTL
            var cacheKey = ActivationTokenCacheKey(request.Email);
            await cacheService.SetAsync(cacheKey, activationData, TimeSpan.FromMinutes(ActivationTokenTtlMinutes));

            logger.LogInformation("✅ [Redis Activation Token] Activation token with email data generated and stored for {Email} with TTL {Ttl} minutes", 
                request.Email, ActivationTokenTtlMinutes);

            return request.Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [Redis Activation Token] Failed to generate activation token with email data for {Email}", request.Email);
            throw;
        }
    }

    public async Task<ActivationTokenData?> GetActivationTokenAsync(string email)
    {
        try
        {
            var cacheKey = ActivationTokenCacheKey(email);
            var activationData = await cacheService.GetAsync<ActivationTokenData>(cacheKey);

            if (activationData == null)
            {
                logger.LogDebug("🔍 [Redis Activation Token] No activation token found for {Email}", email);
                return null;
            }

            // Check if token is expired
            if (activationData.ExpiresAt < DateTime.UtcNow)
            {
                logger.LogWarning("⏰[Redis Activation Token] Activation token expired for {Email}", email);
                await RemoveActivationTokenAsync(email);
                return null;
            }

            logger.LogDebug("✅ [Redis Activation Token] Activation token retrieved for {Email}", email);
            return activationData;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [Redis Activation Token] Failed to get activation token for {Email}", email);
            return null;
        }
    }

    public async Task<bool> ValidateAndConsumeActivationTokenAsync(string email, string token)
    {
        try
        {
            var activationData = await GetActivationTokenAsync(email);
            
            if (activationData == null)
            {
                logger.LogWarning("⚠️ [Redis Activation Token] No activation token found for {Email}", email);
                return false;
            }

            if (activationData.IsUsed)
            {
                logger.LogWarning("⚠️ [Redis Activation Token] Activation token already used for {Email}", email);
                return false;
            }

            if (activationData.Token != token)
            {
                logger.LogWarning("⚠️ [Redis Activation Token] Invalid activation token for {Email}", email);
                return false;
            }

            // Mark token as used
            activationData.IsUsed = true;
            var cacheKey = ActivationTokenCacheKey(email);
            await cacheService.SetAsync(cacheKey, activationData, TimeSpan.FromMinutes(ActivationTokenTtlMinutes));

            logger.LogInformation("✅ [Redis Activation Token] Activation token validated and consumed for {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [Redis Activation Token] Failed to validate activation token for {Email}", email);
            return false;
        }
    }

    public async Task<bool> HasActiveActivationTokenAsync(string email)
    {
        try
        {
            var activationData = await GetActivationTokenAsync(email);
            return activationData != null && !activationData.IsUsed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [Redis Activation Token] Failed to check active activation token for {Email}", email);
            return false;
        }
    }

    public async Task RemoveActivationTokenAsync(string email)
    {
        try
        {
            var cacheKey = ActivationTokenCacheKey(email);
            await cacheService.RemoveAsync(cacheKey);
            logger.LogInformation("🗑️ [Redis Activation Token] Activation token removed for {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [Redis Activation Token] Failed to remove activation token for {Email}", email);
        }
    }


    public async Task<ActivationTokenData?> GetActivationTokenDataAsync(string email)
    {
        return await GetActivationTokenAsync(email);
    }
}
