using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Services.Auth;
using Xunit;

namespace QimErp.Shared.Common.Tests.Auth;

public sealed class UserContextRoleResolutionTests : IDisposable
{
    private const string TenantId = "019e31ec-role-1111-0000-000000000001";

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private readonly FakeHttpContextAccessor _accessor = new();
    private readonly UserContextService _sut;

    public UserContextRoleResolutionTests()
    {
        _sut = new UserContextService(_accessor, NullLogger<UserContextService>.Instance);
        _sut.ClearContext();
    }

    public void Dispose() => _sut.ClearContext();

    private void GivenHttpUserWithRoles(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new("TenantId", TenantId),
            new(ClaimTypes.NameIdentifier, "http-user-001"),
            new(ClaimTypes.Email, "http-user@qimerp.com"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        _accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };
    }

    private static bool CallerIsManager(ICurrentUserService user) =>
        user.AreRolesKnown() &&
        user.GetUserRoles().Any(r => r.Equals("POS_MANAGER", StringComparison.OrdinalIgnoreCase));

    [Fact(DisplayName = "Background/AsyncLocal context reports roles as NOT known")]
    public void Background_context_reports_roles_not_known()
    {
        GivenHttpUserWithRoles("POS_MANAGER");
        _sut.SetContext(TenantId, "bg@system", "BG Worker", "system");

        _sut.AreRolesKnown.Should().BeFalse();
        ((ICurrentUserService)_sut).AreRolesKnown().Should().BeFalse();
        _sut.GetUserRoles().Should().BeEmpty();
    }

    [Fact(DisplayName = "Background/AsyncLocal context cannot pass a role check it should fail")]
    public void Background_context_cannot_pass_role_check()
    {
        GivenHttpUserWithRoles("POS_TELLER");
        _sut.SetContext(TenantId, "bg@system", "BG Worker", "system");

        CallerIsManager(_sut).Should().BeFalse();
    }

    [Fact(DisplayName = "Background/AsyncLocal context is not granted the role even when the ambient HTTP principal holds it")]
    public void Background_context_does_not_inherit_http_role()
    {
        GivenHttpUserWithRoles("POS_MANAGER");
        _sut.SetContext(TenantId, "bg@system", "BG Worker", "system");

        CallerIsManager(_sut).Should().BeFalse();
    }

    [Fact(DisplayName = "HTTP request holding the role passes the role check")]
    public void Http_request_with_role_passes()
    {
        GivenHttpUserWithRoles("POS_TELLER", "POS_MANAGER");

        _sut.AreRolesKnown.Should().BeTrue();
        _sut.GetUserRoles().Should().Contain("POS_MANAGER");
        CallerIsManager(_sut).Should().BeTrue();
    }

    [Fact(DisplayName = "HTTP request without the role fails the role check")]
    public void Http_request_without_role_fails()
    {
        GivenHttpUserWithRoles("POS_TELLER");

        _sut.AreRolesKnown.Should().BeTrue();
        CallerIsManager(_sut).Should().BeFalse();
    }

    [Fact(DisplayName = "HTTP request with no role claims at all reports roles KNOWN and empty")]
    public void Http_request_with_no_roles_is_known_and_empty()
    {
        GivenHttpUserWithRoles();

        _sut.AreRolesKnown.Should().BeTrue();
        _sut.GetUserRoles().Should().BeEmpty();
        CallerIsManager(_sut).Should().BeFalse();
    }

    [Fact(DisplayName = "ClearContext restores HTTP role resolution")]
    public void Clear_context_restores_http_roles()
    {
        GivenHttpUserWithRoles("POS_MANAGER");
        _sut.SetContext(TenantId, "bg@system", "BG Worker", "system");
        CallerIsManager(_sut).Should().BeFalse();

        _sut.ClearContext();

        _sut.AreRolesKnown.Should().BeTrue();
        CallerIsManager(_sut).Should().BeTrue();
    }

    [Fact(DisplayName = "An implementation that always resolves roles reports them as known")]
    public void Non_http_implementation_reports_roles_known()
    {
        ICurrentUserService consumer = new AlwaysResolvesRoles();

        consumer.AreRolesKnown().Should().BeTrue();
    }

    private sealed class AlwaysResolvesRoles : ICurrentUserService
    {
        public string GetCorrelationId() => string.Empty;
        public void SetContext(string tenantId, string userEmail, string? userName = null, string? userId = null) { }
        public void ClearContext() { }
        public bool IsAuthenticated => true;
        public string GetUserId() => "consumer";
        public string? GetRole() => "Consumer";
        public List<string> GetUserRoles() => ["Consumer"];
        public string GetTenantId() => TenantId;
        public string? GetToken() => null;
        public IEnumerable<Claim> GetClaims() => [];
        public string GetUserEmail() => "consumer@system";
        public string GetUserName() => "Consumer";
        public string? GetDomainName() => null;
        public string? GetLanguage() => null;
        public string? GetTimeZone() => null;
        public string? GetCompanyName() => null;
        public string? GetEmployeeId() => null;
        public string? GetRankId() => null;
        public string? GetRankName() => null;
        public string? GetOrganizationalUnitId() => null;
        public string? GetOrganizationalUnitName() => null;
        public List<string> GetRoleIds() => [];
    }
}
