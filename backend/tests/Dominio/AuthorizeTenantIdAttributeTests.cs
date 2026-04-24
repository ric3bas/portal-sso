using Portal.Domain.Base;

namespace sso.global;

public class AuthorizeTenantIdAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetPolicyToTenantIdPolicy()
    {
        var attribute = new AuthorizeTenantIdAttribute();

        Assert.Equal("TenantIdPolicy", attribute.Policy);
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        var attribute = new AuthorizeTenantIdAttribute();

        Assert.NotNull(attribute);
        Assert.IsType<AuthorizeTenantIdAttribute>(attribute);
    }

    [Fact]
    public void Constructor_ShouldInheritFromAuthorizeAttribute()
    {
        var attribute = new AuthorizeTenantIdAttribute();

        Assert.IsAssignableFrom<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(attribute);
    }
}
