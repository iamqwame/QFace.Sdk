using System.Security.Claims;

namespace QimErp.Shared.Common.Services.Auth;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private static readonly AsyncLocal<UserContext> Context = new();

    public void SetContext(string tenantId, string userEmail, string? userName = null, string? userId = null)
    {
        Context.Value = new UserContext
        {
            TenantId = tenantId,
            UserId = userId ?? "system",
            UserEmail = userEmail,
            UserName = userName ?? "system",
            Timestamp = DateTime.UtcNow
        };
    }

    public void ClearContext()
    {
        Context.Value = null;
    }

    public string GetUserId()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return Context.Value.UserId;
        
        // Fall back to HTTP context (HTTP requests)
        return GetClaim(ClaimTypes.NameIdentifier) ?? "";
    }

    public string? GetRole()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return null; // No role in background context
        
        // Fall back to HTTP context (HTTP requests)
        return GetClaim(ClaimTypes.Role);
    }

    public List<string> GetUserRoles()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return []; // No roles in background context
        
        // Fall back to HTTP context (HTTP requests)
        return Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public string GetTenantId()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return Context.Value.TenantId ?? string.Empty;
        
        // Try to get from claims first
        var tenantIdFromClaims = GetClaim("tenantId") ?? GetClaim("TenantId");
        if (!string.IsNullOrWhiteSpace(tenantIdFromClaims))
        {
            return tenantIdFromClaims;
        }
        
        // Fallback: Parse JWT token directly if claims aren't available
        var token = GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            var tenantIdFromToken = ExtractTenantIdFromToken(token);
            if (!string.IsNullOrWhiteSpace(tenantIdFromToken))
            {
                return tenantIdFromToken;
            }
        }
        
        return string.Empty;
    }

    public string? GetToken()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return null; // No token in background context
        
        // Fall back to HTTP context (HTTP requests)
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.Replace("Bearer ", string.Empty);
    }

    public IEnumerable<Claim> GetClaims()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
        {
            var context = Context.Value;
            return
            [
                new Claim("TenantId", context.TenantId ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, context.UserId),
                new Claim(ClaimTypes.Email, context.UserEmail ?? string.Empty),
                new Claim(ClaimTypes.Name, context.UserName ?? string.Empty)
            ];
        }
        
        // Fall back to HTTP context (HTTP requests)
        return _httpContextAccessor.HttpContext?.User.Claims ?? [];
    }

    public string GetUserEmail()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return Context.Value.UserEmail ?? string.Empty;
        
        // Fall back to HTTP context (HTTP requests)
        return GetClaim(ClaimTypes.Email) ?? "";
    }

    public string GetUserName()
    {
        // Check AsyncLocal first (background actors)
        if (Context.Value != null)
            return Context.Value.UserName ?? string.Empty;
        
        // Fall back to HTTP context (HTTP requests)
        return GetClaim(ClaimTypes.Name) ?? "";
    }

    public string? GetDomainName()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("domainName") ?? GetClaim("DomainName");
    }

    public string? GetLanguage()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("language") ?? GetClaim("Language");
    }

    public string? GetTimeZone()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("timeZone") ?? GetClaim("TimeZone");
    }

    public string? GetCompanyName()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("companyName") ?? GetClaim("CompanyName");
    }

    public string? GetEmployeeId()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("employeeId");
    }

    public string? GetRankId()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("rankId");
    }

    public string? GetRankName()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("rankName");
    }

    public string? GetOrganizationalUnitId()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("ouId");
    }

    public string? GetOrganizationalUnitName()
    {
        if (Context.Value != null)
            return null;
        
        return GetClaim("ouName");
    }

    public List<string> GetRoleIds()
    {
        if (Context.Value != null)
            return [];
        
        var roleIds = GetClaim("roleIds");
        if (string.IsNullOrWhiteSpace(roleIds))
            return [];
        
        return roleIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
    
    public bool IsAuthenticated
    {
        get
        {
            var userId = GetUserId();
            return !string.IsNullOrEmpty(userId) && userId != "system" && userId != "anonymous";
        }
    }

    public bool IsStaff =>
        HasActor("staff") || HasAnyRole("Recruiter", "HR", "HiringManager", "Admin");

    public bool IsCandidate =>
        HasActor("candidate") || AudienceContains("careers");

    public bool IsSystem =>
        HasActor("system") || HasAnyScope("system", "svc", "jobs");

    // Helpful parsed IDs (optional)
    public Guid? UserGuid => Guid.TryParse(GetUserId(), out var g) ? g : null;
    public Guid? TenantGuid => Guid.TryParse(GetTenantId(), out var g) ? g : null;

    

    private string? GetClaim(string claimType)
    {
        return Claims.FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
    private bool HasActor(string expected)
    {
        var a = Find("actor") ?? Find("actor_kind");
        return a != null && a.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private bool HasAnyRole(params string[] roles)
    {
        return GetUserRoles().Any(r => roles.Any(x => r.Equals(x, StringComparison.OrdinalIgnoreCase)));
    }
    private bool HasAnyScope(params string[] scopes)
    {
        var scopeVals = Claims.Where(c => c.Type == "scope" || c.Type == "scp")
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return scopeVals.Any(s => scopes.Any(x => s.Equals(x, StringComparison.OrdinalIgnoreCase)));
    }

    private bool AudienceContains(string value)
    {
        var aud = Claims.Where(c => c.Type == "aud").Select(c => c.Value);
        return aud.Any(a => a.Contains(value, StringComparison.OrdinalIgnoreCase));
    }
    
    private IEnumerable<Claim> Claims => GetClaims();
    private string? Find(string type) => Claims.FirstOrDefault(c => c.Type == type)?.Value;

    /// <summary>
    /// Extracts tenantId from JWT token payload by decoding the token
    /// </summary>
    private string? ExtractTenantIdFromToken(string token)
    {
        try
        {
            // JWT tokens have 3 parts separated by dots: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null; // Invalid JWT format
            }

            // Decode the payload (second part)
            var payload = parts[1];
            
            // Add padding if needed (Base64Url encoding may not have padding)
            var padding = payload.Length % 4;
            if (padding != 0)
            {
                payload += new string('=', 4 - padding);
            }
            
            // Replace Base64Url characters with Base64 characters
            payload = payload.Replace('-', '+').Replace('_', '/');
            
            // Decode from Base64
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            
            // Parse JSON to extract tenantId
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;
            
            // Try different possible claim names for tenantId
            if (root.TryGetProperty("tenantId", out var tenantIdProp))
            {
                return tenantIdProp.GetString();
            }
            
            if (root.TryGetProperty("TenantId", out var tenantIdProp2))
            {
                return tenantIdProp2.GetString();
            }
            
            if (root.TryGetProperty("tenant_id", out var tenantIdProp3))
            {
                return tenantIdProp3.GetString();
            }
        }
        catch (Exception)
        {
            // Silently fail - token parsing errors shouldn't break the application
            // Logging can be added here if needed for debugging
        }
        
        return null;
    }

    private class UserContext
    {
        public string? TenantId { get; set; }
        public string UserId { get; set; } = "system";
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

public static class CurrentUserServiceExtensions
    {
        // -------------------------
        // Core helpers
        // -------------------------
        public static IEnumerable<Claim> Claims(this ICurrentUserService u) =>
            u?.GetClaims() ?? Array.Empty<Claim>();

        public static string? GetClaim(this ICurrentUserService u, string type) =>
            u.Claims().FirstOrDefault(c => c.Type == type)?.Value;

        public static bool TryGetClaim(this ICurrentUserService u, string type, out string? value)
        {
            value = u.GetClaim(type);
            return !string.IsNullOrEmpty(value);
        }

        // -------------------------
        // Roles
        // -------------------------
        public static List<string> GetRoles(this ICurrentUserService u)
        {
            // Prefer concrete (uses Distinct + ToList)
            if (u is UserContextService concrete)
                return concrete.GetUserRoles();

            // Fallback compute from claims
            return u.Claims()
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool HasRole(this ICurrentUserService u, string role) =>
            u.GetRoles().Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));

        public static bool HasAnyRole(this ICurrentUserService u, params string[] roles)
        {
            if (roles is null || roles.Length == 0) return false;
            var set = u.GetRoles();
            return roles.Any(r => set.Any(x => x.Equals(r, StringComparison.OrdinalIgnoreCase)));
        }

        public static bool HasAllRoles(this ICurrentUserService u, params string[] roles)
        {
            if (roles is null || roles.Length == 0) return true;
            var set = u.GetRoles();
            return roles.All(r => set.Any(x => x.Equals(r, StringComparison.OrdinalIgnoreCase)));
        }

        // -------------------------
        // Scopes (OAuth2/OIDC)
        // -------------------------
        public static List<string> GetScopes(this ICurrentUserService u)
        {
            return u.Claims()
                .Where(c => c.Type == "scope" || c.Type == "scp")
                .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool HasScope(this ICurrentUserService u, string scope) =>
            u.GetScopes().Any(s => s.Equals(scope, StringComparison.OrdinalIgnoreCase));

        public static bool HasAnyScope(this ICurrentUserService u, params string[] scopes)
        {
            if (scopes is null || scopes.Length == 0) return false;
            var set = u.GetScopes();
            return scopes.Any(s => set.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)));
        }

        // -------------------------
        // Audiences
        // -------------------------
        public static List<string> GetAudiences(this ICurrentUserService u) =>
            u.Claims().Where(c => c.Type == "aud").Select(c => c.Value).ToList();

        public static bool AudienceContains(this ICurrentUserService u, string value) =>
            u.GetAudiences().Any(a => a?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

        // -------------------------
        // Actor flags (computed if not concrete)
        // -------------------------
        public static bool IsAuthenticated(this ICurrentUserService u)
        {
            if (u is UserContextService concrete) return concrete.IsAuthenticated;
            var userId = u.GetUserId();
            return !string.IsNullOrEmpty(userId) && userId != "system" && userId != "anonymous";
        }

        public static bool IsStaff(this ICurrentUserService u)
        {
            if (u is UserContextService concrete) return concrete.IsStaff;
            return u.HasAnyRole("Recruiter", "HR", "HiringManager", "Admin")
                   || u.GetClaim("actor")?.Equals("staff", StringComparison.OrdinalIgnoreCase) == true
                   || u.GetClaim("actor_kind")?.Equals("staff", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsCandidate(this ICurrentUserService u)
        {
            if (u is UserContextService concrete) return concrete.IsCandidate;
            return u.GetClaim("actor")?.Equals("candidate", StringComparison.OrdinalIgnoreCase) == true
                   || u.GetClaim("actor_kind")?.Equals("candidate", StringComparison.OrdinalIgnoreCase) == true
                   || u.AudienceContains("careers");
        }

        public static bool IsSystem(this ICurrentUserService u)
        {
            if (u is UserContextService concrete) return concrete.IsSystem;
            return u.GetClaim("actor")?.Equals("system", StringComparison.OrdinalIgnoreCase) == true
                   || u.GetClaim("actor_kind")?.Equals("system", StringComparison.OrdinalIgnoreCase) == true
                   || u.HasAnyScope("system", "svc", "jobs");
        }

        // -------------------------
        // IDs + Identity info
        // -------------------------
        public static Guid? UserGuid(this ICurrentUserService u)
        {
            var id = u.GetUserId();
            return Guid.TryParse(id, out var g) ? g : null;
        }

        public static Guid? TenantGuid(this ICurrentUserService u)
        {
            var id = u.GetTenantId();
            return Guid.TryParse(id, out var g) ? g : null;
        }

        /// <summary>
        /// Preferred: explicit "candidate_id"/"cid" claim; fallback to UserGuid when IsCandidate.
        /// </summary>
        public static Guid? CandidateGuid(this ICurrentUserService u)
        {
            var cid = u.GetClaim("candidate_id") ?? u.GetClaim("cid");
            if (Guid.TryParse(cid, out var g)) return g;
            return u.IsCandidate() ? u.UserGuid() : null;
        }

        // -------------------------
        // Email/Name shortcuts
        // -------------------------
        public static string? Email(this ICurrentUserService u)
        {
            var e = u.GetUserEmail();
            return string.IsNullOrWhiteSpace(e) ? (u.GetClaim("email") ?? u.GetClaim(ClaimTypes.Email)) : e;
        }

        public static string? FullName(this ICurrentUserService u)
        {
            var n = u.GetUserName();
            return string.IsNullOrWhiteSpace(n) ? (u.GetClaim("name") ?? u.GetClaim(ClaimTypes.Name)) : n;
        }
    }