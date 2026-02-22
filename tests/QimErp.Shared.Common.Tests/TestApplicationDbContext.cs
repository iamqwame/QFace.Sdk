using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Tests;

/// <summary>
/// Test ApplicationDbContext for DI resolution tests.
/// </summary>
public class TestApplicationDbContext : ApplicationDbContext<TestApplicationDbContext>
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }
}
