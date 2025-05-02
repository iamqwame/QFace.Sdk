using System.Security.Claims;
using QFace.Sdk.Extensions.Services;

namespace Demo.MongoDb.MultiTenant.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(
        IUserRepository userRepository,
        ITenantUserRepository tenantUserRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ITenantAccessor tenantAccessor,
        ILogger<AuthController> logger)
        : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Invalid login request");

            try
            {
                // Find user by email
                var user = await userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                    return Unauthorized(AuthResult.Failed("Invalid email or password"));

                // Verify password
                if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                    return Unauthorized(AuthResult.Failed("Invalid email or password"));

                // Get user's tenants
                var tenantUsers = await tenantUserRepository.GetTenantsByUserIdAsync(user.Id);
                
                // If tenantCode is specified, validate access to that tenant
                string? targetTenantId = null;
                if (!string.IsNullOrEmpty(request.TenantCode))
                {
                    var tenant = await tenantRepository.GetByCodeAsync(request.TenantCode);
                    if (tenant == null)
                        return BadRequest(AuthResult.Failed("Tenant not found"));

                    targetTenantId = tenant.Id;
                    
                    // Check if user has access to this tenant
                    var tenantAccess = tenantUsers.FirstOrDefault(tu => tu.TenantId == targetTenantId);
                    if (tenantAccess == null)
                        return Unauthorized(AuthResult.Failed("You don't have access to this tenant"));

                    if (tenantAccess.IsCurrentlyLocked())
                        return Unauthorized(AuthResult.Failed("Your account is locked"));

                    if (tenantAccess.HasAccessExpired())
                        return Unauthorized(AuthResult.Failed("Your access to this tenant has expired"));

                    // Record successful login
                    tenantAccess.RecordSuccessfulLogin();
                    await tenantUserRepository.UpdateAsync(tenantAccess);
                }
                // If no tenant specified but user has tenants, use primary or first tenant
                else if (tenantUsers.Any())
                {
                    var primaryTenant = tenantUsers.FirstOrDefault(tu => tu.IsPrimaryTenant) ?? tenantUsers.First();
                    targetTenantId = primaryTenant.TenantId;
                }
                
                if (targetTenantId == null)
                    return BadRequest(AuthResult.Failed("No available tenant"));

                // Get the tenant for response
                var targetTenant = await tenantRepository.GetByIdAsync(targetTenantId);
                if (targetTenant == null)
                    return BadRequest(AuthResult.Failed("Tenant not found"));

                // Get user's role and permissions for this tenant
                var tenantUser = tenantUsers.First(tu => tu.TenantId == targetTenantId);

                // Generate tokens using the specific method from ITokenService
                var (token, expires) = tokenService.GenerateAccessToken(
                    user.Id,
                    targetTenantId,
                    targetTenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions
                );
                
                var (refreshToken, refreshExpires) = tokenService.GenerateRefreshToken();

                // Save refresh token
                await tenantAccessor.ExecuteWithTenantAsync(targetTenantId, async () =>
                {
                    await refreshTokenRepository.InsertOneAsync(new RefreshToken
                    {
                        Token = refreshToken,
                        UserId = user.Id,
                        ExpiresAt = refreshExpires,
                        ClientId = request.ClientId ?? "default",
                        DeviceInfo = request.DeviceInfo ?? "unknown",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                    });
                });

                // Prepare list of available tenants
                var availableTenants = new List<TenantSummary>();
                foreach (var tu in tenantUsers)
                {
                    var t = await tenantRepository.GetByIdAsync(tu.TenantId);
                    if (t != null && t.IsActive && t.IsProvisioned)
                    {
                        availableTenants.Add(new TenantSummary
                        {
                            Id = t.Id,
                            Code = t.Code,
                            Name = t.Name,
                            Role = tu.Role,
                            IsPrimary = tu.IsPrimaryTenant
                        });
                    }
                }

                // Create auth result
                var authResult = new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = targetTenantId,
                    TenantCode = targetTenant.Code,
                    TenantName = targetTenant.Name,
                    AccessToken = token,
                    AccessTokenExpiresAt = expires,
                    RefreshToken = refreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = false,
                    AvailableTenants = availableTenants
                };

                return Ok(authResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login error");
                return StatusCode(500, AuthResult.Failed("An error occurred during login"));
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("Invalid refresh token request");

            try
            {
                // Validate refresh token
                var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
                if (refreshToken == null || !refreshToken.IsActive)
                    return Unauthorized(AuthResult.Failed("Invalid refresh token"));

                // Get user
                var user = await userRepository.GetByIdAsync(refreshToken.UserId);
                if (user == null)
                    return Unauthorized(AuthResult.Failed("User not found"));

                // Set tenant context from the refresh token
                tenantAccessor.SetCurrentTenantId(refreshToken.TenantId);

                // Get tenant
                var tenant = await tenantRepository.GetByIdAsync(refreshToken.TenantId);
                if (tenant == null)
                    return Unauthorized(AuthResult.Failed("Tenant not found"));

                // Get tenant user for permissions
                var tenantUser = await tenantUserRepository.GetTenantUserAsync(user.Id, refreshToken.TenantId);
                if (tenantUser == null)
                    return Unauthorized(AuthResult.Failed("User does not have access to this tenant"));

                // Generate new tokens
                var (newAccessToken, expires) = tokenService.GenerateAccessToken(
                    user.Id,
                    refreshToken.TenantId,
                    tenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions
                );
                
                var (newRefreshToken, refreshExpires) = tokenService.GenerateRefreshToken();

                // Revoke old refresh token
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevocationReason = "Token refresh";
                refreshToken.ReplacedByToken = newRefreshToken;
                await refreshTokenRepository.UpdateAsync(refreshToken);

                // Save new refresh token
                await refreshTokenRepository.InsertOneAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    TenantId = refreshToken.TenantId,
                    ExpiresAt = refreshExpires,
                    ClientId = refreshToken.ClientId,
                    DeviceInfo = refreshToken.DeviceInfo,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                });

                // Get all tenants for the user
                var tenantUsers = await tenantUserRepository.GetTenantsByUserIdAsync(user.Id);
                var availableTenants = new List<TenantSummary>();
                foreach (var tu in tenantUsers)
                {
                    var t = await tenantRepository.GetByIdAsync(tu.TenantId);
                    if (t != null && t.IsActive && t.IsProvisioned)
                    {
                        availableTenants.Add(new TenantSummary
                        {
                            Id = t.Id,
                            Code = t.Code,
                            Name = t.Name,
                            Role = tu.Role,
                            IsPrimary = tu.IsPrimaryTenant
                        });
                    }
                }

                // Create auth result
                var authResult = new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = refreshToken.TenantId,
                    TenantCode = tenant.Code,
                    TenantName = tenant.Name,
                    AccessToken = newAccessToken,
                    AccessTokenExpiresAt = expires,
                    RefreshToken = newRefreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = false,
                    AvailableTenants = availableTenants
                };

                return Ok(authResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refresh token error");
                return StatusCode(500, AuthResult.Failed("An error occurred during token refresh"));
            }
        }

        [HttpPost("switch-tenant")]
        [Authorize]
        public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TenantId))
                return BadRequest("Invalid switch tenant request");

            try
            {
                // Get the current user ID from claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(AuthResult.Failed("User not authenticated"));

                // Get user
                var user = await userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Unauthorized(AuthResult.Failed("User not found"));

                // Check if user has access to this tenant
                var tenantUser = await tenantUserRepository.GetTenantUserAsync(userId, request.TenantId);
                if (tenantUser == null)
                    return Unauthorized(AuthResult.Failed("You don't have access to this tenant"));

                // Get tenant
                var tenant = await tenantRepository.GetByIdAsync(request.TenantId);
                if (tenant == null)
                    return BadRequest(AuthResult.Failed("Tenant not found"));

                // Generate new tokens
                var (newAccessToken, expires) = tokenService.GenerateAccessToken(
                    user.Id,
                    tenant.Id,
                    tenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions
                );
                
                var (newRefreshToken, refreshExpires) = tokenService.GenerateRefreshToken();

                // Set tenant context for refresh token
                tenantAccessor.SetCurrentTenantId(tenant.Id);

                // Save new refresh token
                await refreshTokenRepository.InsertOneAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = refreshExpires,
                    ClientId = request.ClientId ?? "default",
                    DeviceInfo = request.DeviceInfo ?? "unknown",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                });

                // Get all tenants for the user
                var tenantUsers = await tenantUserRepository.GetTenantsByUserIdAsync(user.Id);
                var availableTenants = new List<TenantSummary>();
                foreach (var tu in tenantUsers)
                {
                    var t = await tenantRepository.GetByIdAsync(tu.TenantId);
                    if (t != null && t.IsActive && t.IsProvisioned)
                    {
                        availableTenants.Add(new TenantSummary
                        {
                            Id = t.Id,
                            Code = t.Code,
                            Name = t.Name,
                            Role = tu.Role,
                            IsPrimary = tu.IsPrimaryTenant
                        });
                    }
                }

                // Create auth result
                var authResult = new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = tenant.Id,
                    TenantCode = tenant.Code,
                    TenantName = tenant.Name,
                    AccessToken = newAccessToken,
                    AccessTokenExpiresAt = expires,
                    RefreshToken = newRefreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = false,
                    AvailableTenants = availableTenants
                };

                return Ok(authResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Switch tenant error");
                return StatusCode(500, AuthResult.Failed("An error occurred during tenant switch"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("Invalid logout request");

            try
            {
                // Find and revoke the refresh token
                var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
                if (refreshToken != null && refreshToken.IsActive)
                {
                    // Set tenant context
                    tenantAccessor.SetCurrentTenantId(refreshToken.TenantId);
                    
                    // Update token status
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    refreshToken.RevocationReason = "User logout";
                    await refreshTokenRepository.UpdateAsync(refreshToken);
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Logout error");
                return StatusCode(500, new { error = "An error occurred during logout" });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? TenantCode { get; set; }
        public string? ClientId { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class SwitchTenantRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public string? ClientId { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}