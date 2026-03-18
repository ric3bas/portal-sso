using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Portal.Middleware;

namespace sso.global;

public class JwtValidationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var wasCalled = false;
        RequestDelegate next = _ =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var configuration = Substitute.For<IConfiguration>();
        var middleware = new JwtValidationMiddleware(next, configuration);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(wasCalled);
    }
}
