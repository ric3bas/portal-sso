using Microsoft.AspNetCore.Http;
using NSubstitute;
using Portal.Domain.Base;
using System.Security.Claims;

namespace sso.global;

public class BaseServiceTests
{
    private sealed class TestableBaseService : BaseService
    {
        public TestableBaseService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        public new Guid ObterTenantId() => base.ObterTenantId();
    }

    [Fact]
    public void ObterTenantId_WithInvalidGuid_ThrowsUnauthorizedAccessException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim("tenantId", "invalid-guid") });
        httpContext.User = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var service = new TestableBaseService(httpContextAccessor);

        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.ObterTenantId());

        Assert.Equal("TenantId inválido ou ausente no token", exception.Message);
    }

    [Fact]
    public void ObterTenantId_WithEmptyGuid_ThrowsUnauthorizedAccessException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim("tenantId", Guid.Empty.ToString()) });
        httpContext.User = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var service = new TestableBaseService(httpContextAccessor);

        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.ObterTenantId());

        Assert.Equal("TenantId inválido ou ausente no token", exception.Message);
    }

    [Fact]
    public void ObterTenantId_WithNullClaim_ThrowsUnauthorizedAccessException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity();
        httpContext.User = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var service = new TestableBaseService(httpContextAccessor);

        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.ObterTenantId());

        Assert.Equal("TenantId inválido ou ausente no token", exception.Message);
    }

    [Fact]
    public void ObterTenantId_WithNullHttpContext_ThrowsUnauthorizedAccessException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var service = new TestableBaseService(httpContextAccessor);

        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.ObterTenantId());

        Assert.Equal("TenantId inválido ou ausente no token", exception.Message);
    }

    [Fact]
    public void ObterTenantId_WithNullUser_ThrowsUnauthorizedAccessException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = null!;
        httpContextAccessor.HttpContext.Returns(httpContext);
        var service = new TestableBaseService(httpContextAccessor);

        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.ObterTenantId());

        Assert.Equal("TenantId inválido ou ausente no token", exception.Message);
    }

    [Fact]
    public void ObterTenantId_WithValidGuid_ReturnsTenantId()
    {
        var expectedTenantId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim("tenantId", expectedTenantId.ToString()) });
        httpContext.User = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var service = new TestableBaseService(httpContextAccessor);

        var result = service.ObterTenantId();

        Assert.Equal(expectedTenantId, result.Data);
    }
}
