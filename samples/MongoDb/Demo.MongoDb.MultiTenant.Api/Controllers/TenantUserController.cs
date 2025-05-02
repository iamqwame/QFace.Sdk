using QFace.Sdk.Extensions.Services;

namespace Demo.MongoDb.MultiTenant.Api.Controllers;

    [ApiController]
    [Route("api/tenant-users")]
    [Authorize(Policy = "TenantAdmin")]
    public class TenantUserController(
        ITenantUserRepository tenantUserRepository,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        ITenantAccessor tenantAccessor,
        ILogger<TenantUserController> logger)
        : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository = tenantRepository;

        [HttpGet]
        public async Task<IActionResult> GetTenantUsers()
        {
            try
            {
                var tenantId = tenantAccessor.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("No tenant context available");

                var tenantUsers = await tenantUserRepository.GetUsersByTenantIdAsync(tenantId);
                
                // Map to a view model with user details
                var userDetails = new List<TenantUserViewModel>();
                foreach (var tu in tenantUsers)
                {
                    var user = await userRepository.GetByIdAsync(tu.UserId);
                    if (user != null)
                    {
                        userDetails.Add(new TenantUserViewModel
                        {
                            Id = tu.Id,
                            UserId = tu.UserId,
                            TenantId = tu.TenantId,
                            Email = user.Email,
                            FullName = user.FullName,
                            Role = tu.Role,
                            Permissions = tu.Permissions,
                            IsActive = tu.IsActive,
                            IsPrimaryTenant = tu.IsPrimaryTenant,
                            AddedDate = tu.AddedDate,
                            RequiresMfa = tu.RequiresMfa,
                            MfaConfigured = tu.MfaConfigured,
                            LastLoginDate = tu.LastLoginDate
                        });
                    }
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tenant users");
                return StatusCode(500, new { error = "An error occurred while retrieving tenant users" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenantUserById(string id)
        {
            try
            {
                var tenantId = tenantAccessor.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("No tenant context available");

                var tenantUser = await tenantUserRepository.GetByIdAsync(id);
                if (tenantUser == null || tenantUser.TenantId != tenantId)
                    return NotFound();

                var user = await userRepository.GetByIdAsync(tenantUser.UserId);
                if (user == null)
                    return NotFound();

                var viewModel = new TenantUserViewModel
                {
                    Id = tenantUser.Id,
                    UserId = tenantUser.UserId,
                    TenantId = tenantUser.TenantId,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    IsActive = tenantUser.IsActive,
                    IsPrimaryTenant = tenantUser.IsPrimaryTenant,
                    AddedDate = tenantUser.AddedDate,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaConfigured = tenantUser.MfaConfigured,
                    LastLoginDate = tenantUser.LastLoginDate
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tenant user {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the tenant user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToTenant([FromBody] AddTenantUserRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            try
            {
                var tenantId = tenantAccessor.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("No tenant context available");

                // Check if user exists
                User user;
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    // Adding existing user
                    user = await userRepository.GetByIdAsync(request.UserId);
                    if (user == null)
                        return BadRequest("User not found");
                }
                else if (!string.IsNullOrEmpty(request.Email))
                {
                    // Check if user with this email already exists
                    user = await userRepository.GetByEmailAsync(request.Email);
                    
                    if (user == null && !string.IsNullOrEmpty(request.Password))
                    {
                        // Create new user
                        (string passwordHash, string passwordSalt) = passwordHasher.HashPassword(request.Password);
                        
                        user = new User
                        {
                            Email = request.Email,
                            Username = request.Email,
                            FullName = request.FullName ?? request.Email,
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                            IsEmailVerified = false
                        };
                        
                        await userRepository.InsertOneAsync(user);
                    }
                    else if (user == null)
                    {
                        return BadRequest("User doesn't exist and no password provided to create one");
                    }
                }
                else
                {
                    return BadRequest("Either userId or email must be provided");
                }

                // Check if user is already in tenant
                var existingTenantUser = await tenantUserRepository.GetTenantUserAsync(user.Id, tenantId);
                if (existingTenantUser != null)
                {
                    return BadRequest("User is already a member of this tenant");
                }

                // Add user to tenant
                var tenantUser = new TenantUser
                {
                    UserId = user.Id,
                    TenantId = tenantId,
                    Role = request.Role ?? "User",
                    Permissions = request.Permissions ?? new List<string>(),
                    AddedDate = DateTime.UtcNow,
                    AddedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system",
                    IsPrimaryTenant = request.IsPrimaryTenant,
                    RequiresMfa = request.RequiresMfa
                };

                await tenantUserRepository.InsertOneAsync(tenantUser);

                var viewModel = new TenantUserViewModel
                {
                    Id = tenantUser.Id,
                    UserId = tenantUser.UserId,
                    TenantId = tenantUser.TenantId,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    IsActive = tenantUser.IsActive,
                    IsPrimaryTenant = tenantUser.IsPrimaryTenant,
                    AddedDate = tenantUser.AddedDate,
                    RequiresMfa = tenantUser.RequiresMfa
                };

                return CreatedAtAction(nameof(GetTenantUserById), new { id = tenantUser.Id }, viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding user to tenant");
                return StatusCode(500, new { error = "An error occurred while adding the user to the tenant" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenantUser(string id, [FromBody] UpdateTenantUserRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            try
            {
                var tenantId = tenantAccessor.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("No tenant context available");

                var tenantUser = await tenantUserRepository.GetByIdAsync(id);
                if (tenantUser == null || tenantUser.TenantId != tenantId)
                    return NotFound();

                // Update properties
                if (!string.IsNullOrEmpty(request.Role))
                    tenantUser.Role = request.Role;

                if (request.Permissions != null)
                    tenantUser.Permissions = request.Permissions;

                if (request.IsActive.HasValue)
                    tenantUser.IsActive = request.IsActive.Value;

                if (request.IsPrimaryTenant.HasValue)
                    tenantUser.IsPrimaryTenant = request.IsPrimaryTenant.Value;

                if (request.RequiresMfa.HasValue)
                    tenantUser.RequiresMfa = request.RequiresMfa.Value;

                // Update the tenant user
                var updated = await tenantUserRepository.UpdateAsync(tenantUser);
                if (!updated)
                    return StatusCode(500, new { error = "Failed to update tenant user" });

                var user = await userRepository.GetByIdAsync(tenantUser.UserId);
                
                var viewModel = new TenantUserViewModel
                {
                    Id = tenantUser.Id,
                    UserId = tenantUser.UserId,
                    TenantId = tenantUser.TenantId,
                    Email = user?.Email ?? string.Empty,
                    FullName = user?.FullName ?? string.Empty,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    IsActive = tenantUser.IsActive,
                    IsPrimaryTenant = tenantUser.IsPrimaryTenant,
                    AddedDate = tenantUser.AddedDate,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaConfigured = tenantUser.MfaConfigured,
                    LastLoginDate = tenantUser.LastLoginDate
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating tenant user {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating the tenant user" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserFromTenant(string id)
        {
            try
            {
                var tenantId = tenantAccessor.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return BadRequest("No tenant context available");

                var tenantUser = await tenantUserRepository.GetByIdAsync(id);
                if (tenantUser == null || tenantUser.TenantId != tenantId)
                    return NotFound();

                // Don't allow removing the last admin
                if (tenantUser.Role == "TenantAdmin")
                {
                    var admins = await tenantUserRepository.FindAsync(
                        tu => tu.TenantId == tenantId && tu.Role == "TenantAdmin" && tu.IsActive);
                    
                    var adminsList = admins.ToList();
                    if (adminsList.Count <= 1)
                        return BadRequest("Cannot remove the last tenant admin");
                }

                var deleted = await tenantUserRepository.DeleteByIdAsync(id);
                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing user from tenant {Id}", id);
                return StatusCode(500, new { error = "An error occurred while removing the user from the tenant" });
            }
        }
    }

    public class TenantUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; }
        public bool IsPrimaryTenant { get; set; }
        public DateTime AddedDate { get; set; }
        public bool RequiresMfa { get; set; }
        public bool MfaConfigured { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class AddTenantUserRequest
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public List<string>? Permissions { get; set; }
        public bool IsPrimaryTenant { get; set; }
        public bool RequiresMfa { get; set; }
    }

    public class UpdateTenantUserRequest
    {
        public string? Role { get; set; }
        public List<string>? Permissions { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimaryTenant { get; set; }
        public bool? RequiresMfa { get; set; }
    }