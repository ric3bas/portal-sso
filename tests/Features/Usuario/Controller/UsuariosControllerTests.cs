using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Portal.Features.Usuario.Controller;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;

namespace sso.controllers;

public class UsuariosControllerTests
{
    private static readonly Type _controller = typeof(UsuariosController);

    [Fact]
    public void Constructor_SetsUsuarioService()
    {
        var service = Substitute.For<IUsuarioService>();

        var controller = new UsuariosController(service);

        Assert.NotNull(controller);
    }

    [Fact]
    public void Controller_HasExpectedClassAttributes()
    {
        Assert.True(_controller.GetCustomAttributes(typeof(ApiControllerAttribute), false).Length > 0);
        Assert.True(_controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Length > 0);
        Assert.True(_controller.GetCustomAttributes(typeof(ApiVersionAttribute), false).Length > 0);
        Assert.True(_controller.GetCustomAttributes(typeof(RouteAttribute), false).Length > 0);
    }

    [Theory]
    [InlineData("ListarAsync", typeof(HttpGetAttribute))]
    [InlineData("RegisterAsync", typeof(HttpPostAttribute))]
    public void Endpoint_Exists(string methodName, Type httpAttribute)
    {
        var method = _controller.GetMethod(methodName);

        Assert.NotNull(method);
        Assert.True(method.GetCustomAttributes(httpAttribute, false).Length > 0);
    }

    [Fact]
    public async Task ListarAsync_Returns200WithPayload()
    {
        var service = Substitute.For<IUsuarioService>();
        var payload = new List<UsuarioComPerfilResponse>
        {
            new() { Id = 1, Nome = "Ricardo", Login = "ricardo", Email = "ricardo@email.com" }
        };

        service.ListarAsync(Arg.Any<CancellationToken>()).Returns(payload);

        var controller = new UsuariosController(service);

        var result = await controller.ListarAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(payload, okResult.Value);
    }

    [Fact]
    public async Task ListarAsync_CallsServiceWithSameCancellationToken()
    {
        var service = Substitute.For<IUsuarioService>();
        service.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<UsuarioComPerfilResponse>());
        var controller = new UsuariosController(service);
        using var cts = new CancellationTokenSource();

        await controller.ListarAsync(cts.Token);

        await service.Received(1).ListarAsync(cts.Token);
    }

    [Fact]
    public async Task ListarAsync_WhenServiceThrows_PropagatesException()
    {
        var service = Substitute.For<IUsuarioService>();
        service.ListarAsync(Arg.Any<CancellationToken>()).Throws(new InvalidOperationException("erro"));
        var controller = new UsuariosController(service);

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ListarAsync(CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Returns200()
    {
        var service = Substitute.For<IUsuarioService>();
        service.RegisterAsync(Arg.Any<RegisterRequest>())
               .Returns(Task.CompletedTask);

        var controller = new UsuariosController(service);

        var result = await controller.RegisterAsync(new RegisterRequest());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("Usuário criado com sucesso", okResult.Value);
    }

    [Fact]
    public async Task RegisterAsync_CallsServiceWithSameRequest()
    {
        var service = Substitute.For<IUsuarioService>();
        service.RegisterAsync(Arg.Any<RegisterRequest>()).Returns(Task.CompletedTask);
        var controller = new UsuariosController(service);
        var request = new RegisterRequest
        {
            Nome = "User Test",
            Login = "user.test",
            Email = "user@test.com",
            Senha = "Senha@123",
            PerfilId = 1
        };

        await controller.RegisterAsync(request);

        await service.Received(1).RegisterAsync(request);
    }

    [Fact]
    public async Task RegisterAsync_WhenServiceThrows_PropagatesException()
    {
        var service = Substitute.For<IUsuarioService>();
        service.RegisterAsync(Arg.Any<RegisterRequest>()).Throws(new InvalidOperationException("erro"));
        var controller = new UsuariosController(service);

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.RegisterAsync(new RegisterRequest()));
    }
}
