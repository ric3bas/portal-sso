using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Portal.Dominio;

namespace sso.global;

public class TenantIdRequirementTests
{
    private readonly TenantIdHandler _handler;
    private readonly TenantIdRequirement _requirement;

    public TenantIdRequirementTests()
    {
        _handler = new TenantIdHandler();
        _requirement = new TenantIdRequirement();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasValidTenantIdClaim_Succeeds()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenantId", "12345")
        }));
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasNoTenantIdClaim_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("someOtherClaim", "value")
        }));
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasEmptyTenantIdClaim_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenantId", "")
        }));
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasWhitespaceTenantIdClaim_Succeeds()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenantId", " ")
        }));
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasMultipleClaimsIncludingValidTenantId_Succeeds()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("name", "John Doe"),
            new Claim("tenantId", "tenant-123"),
            new Claim("role", "Admin")
        }));
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithNoIdentity_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal();
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithNoClaims_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            user,
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
