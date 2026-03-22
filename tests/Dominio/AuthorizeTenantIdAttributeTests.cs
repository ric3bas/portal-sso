using Portal.Domain.Base;

namespace sso.global;

public class AuthorizeTenantIdAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetPolicyToTenantIdPolicy()
    {
        // Act
        var attribute = new AuthorizeTenantIdAttribute();

        // Assert
        Assert.Equal("TenantIdPolicy", attribute.Policy);
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Act
        var attribute = new AuthorizeTenantIdAttribute();

        // Assert
        Assert.NotNull(attribute);
        Assert.IsType<AuthorizeTenantIdAttribute>(attribute);
    }

    [Fact]
    public void Constructor_ShouldInheritFromAuthorizeAttribute()
    {
        // Act
        var attribute = new AuthorizeTenantIdAttribute();

        // Assert
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(attribute);
    }
}
