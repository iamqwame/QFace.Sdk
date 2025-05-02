namespace Demo.MongoDb.MultiTenant.Api;

    [ApiController]
    [Route("api/tenants")]
    public class TenantController(
        ITenantRepository tenantRepository,
        ITenantService tenantService,
        ILogger<TenantController> logger)
        : ControllerBase
    {
        private readonly ILogger<TenantController> _logger = logger;

        [HttpGet]
        [Authorize(Policy = "ManageTenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await tenantRepository.GetAllAsync();
            return Ok(tenants);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ManageTenants")]
        public async Task<IActionResult> GetTenantById(string id)
        {
            var tenant = await tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }

        [HttpGet("by-code/{code}")]
        [Authorize(Policy = "ManageTenants")]
        public async Task<IActionResult> GetTenantByCode(string code)
        {
            var tenant = await tenantRepository.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTenant([FromBody] TenantCreationRequest request)
        {
            if (request == null)
                return BadRequest("Invalid tenant creation request");

            var result = await tenantService.CreateTenantAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Created($"/api/tenants/{result.TenantId}", result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ManageTenants")]
        public async Task<IActionResult> UpdateTenant(string id, [FromBody] TenantInfo request)
        {
            if (request == null)
                return BadRequest("Invalid tenant update request");

            var tenant = await tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound();

            // Update tenant properties
            tenant.Name = request.Name;
            tenant.Description = request.Description;
            
            if (!string.IsNullOrEmpty(request.Code))
            {
                // Check if code is unique
                var existingWithCode = await tenantRepository.GetByCodeAsync(request.Code);
                if (existingWithCode != null && existingWithCode.Id != id)
                    return BadRequest("Tenant code is already in use");
                
                tenant.Code = request.Code;
            }

            // Update contact information
            if (request.Contact != null)
            {
                tenant.Contact.AdminName = request.Contact.AdminName;
                tenant.Contact.AdminEmail = request.Contact.AdminEmail;
                tenant.Contact.AdminPhone = request.Contact.AdminPhone;
                tenant.Contact.CompanyName = request.Contact.CompanyName;
                tenant.Contact.CompanyWebsite = request.Contact.CompanyWebsite;
                
                if (request.Contact.Address != null)
                {
                    tenant.Contact.Address.Street = request.Contact.Address.Street;
                    tenant.Contact.Address.City = request.Contact.Address.City;
                    tenant.Contact.Address.State = request.Contact.Address.State;
                    tenant.Contact.Address.PostalCode = request.Contact.Address.PostalCode;
                    tenant.Contact.Address.Country = request.Contact.Address.Country;
                }
            }

            // Update subscription information
            if (request.Subscription != null)
            {
                tenant.Subscription.Tier = request.Subscription.Tier;
                tenant.Subscription.ExpiryDate = request.Subscription.ExpiryDate;
                tenant.Subscription.IsTrialAccount = request.Subscription.IsTrialAccount;
                tenant.Subscription.MaxUsers = request.Subscription.MaxUsers;
                tenant.Subscription.MaxStorageMB = request.Subscription.MaxStorageMB;
                tenant.Subscription.LastRenewedDate = DateTime.UtcNow;
            }

            var updated = await tenantRepository.UpdateAsync(tenant);
            
            if (!updated)
                return StatusCode(500, "Failed to update tenant");

            return Ok(tenant);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ManageTenants")]
        public async Task<IActionResult> DeleteTenant(string id)
        {
            var deleted = await tenantRepository.DeleteByIdAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }