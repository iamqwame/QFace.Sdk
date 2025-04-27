namespace QFace.Sdk.MongoDb.MultiTenant.Middleware
{
    /// <summary>
    /// Options for tenant resolution middleware
    /// </summary>
    public class TenantResolutionOptions
    {
        /// <summary>
        /// Use route value resolution (tenantId or tenantCode)
        /// </summary>
        public bool UseRouteResolution { get; set; } = true;
        
        /// <summary>
        /// Use header resolution (X-Tenant-ID or X-Tenant-Code)
        /// </summary>
        public bool UseHeaderResolution { get; set; } = true;
        
        /// <summary>
        /// Use query string resolution (?tenantId=xxx or ?tenantCode=xxx)
        /// </summary>
        public bool UseQueryStringResolution { get; set; } = false;
        
        /// <summary>
        /// Use cookie resolution (tenant_id or tenant_code cookies)
        /// </summary>
        public bool UseCookieResolution { get; set; } = false;
        
        /// <summary>
        /// Use auth claim resolution (tenant_id or tenant_code claims)
        /// </summary>
        public bool UseAuthClaimResolution { get; set; } = true;
        
        /// <summary>
        /// Whether to include tenant info in response headers
        /// </summary>
        public bool IncludeTenantInfoInResponse { get; set; } = true;
        
        /// <summary>
        /// Whether to require a tenant for all requests
        /// </summary>
        public bool RequireTenant { get; set; } = false;
        
        /// <summary>
        /// Whether to reject requests with invalid/inactive tenants
        /// </summary>
        public bool RejectInvalidTenants { get; set; } = true;
        
        /// <summary>
        /// Whether to reject requests with unprovisioned tenants
        /// </summary>
        public bool RejectUnprovisionedTenants { get; set; } = true;
        
        /// <summary>
        /// Whether to fail the request on tenant resolution error
        /// </summary>
        public bool FailOnResolutionError { get; set; } = false;
        
        /// <summary>
        /// Paths for which tenant resolution should be skipped
        /// </summary>
        public List<string> ExcludedPaths { get; set; } = new List<string>
        {
            "/health",
            "/metrics",
            "/.well-known",
            "/favicon.ico"
        };
        
        /// <summary>
        /// Paths that are exempt from tenant requirement
        /// </summary>
        public List<string> TenantExemptPaths { get; set; } = new List<string>
        {
            "/api/tenants",
            "/api/auth"
        };
    }
}