using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Portal.Features.Auth.Controller;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;

namespace sso.controllers;

public class AuthControllerTests
{
    private static readonly Type _controller = typeof(AuthController);

    [Fact]
    public void Constructor_SetsServices()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        Assert.NotNull(controller);
    }

    [Theory]
    [InlineData("Login", typeof(HttpPostAttribute))]
    [InlineData("RefreshTokenAsync", typeof(HttpPostAttribute))]
    [InlineData("LogoutAsync", typeof(HttpPostAttribute))]
    [InlineData("Solicitar", typeof(HttpPostAttribute))]
    [InlineData("ValidarToken", typeof(HttpGetAttribute))]
    public void Endpoint_Exists(string methodName, Type httpAttribute)
    {
        var method = _controller.GetMethod(methodName);
        Assert.NotNull(method);
        Assert.True(method.GetCustomAttributes(httpAttribute, false).Length > 0);
    }

    [Fact]
    public async Task Login_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new LoginResponse
        {
            AccessToken = "test_access_token",
            RefreshToken = "test_refresh_token",
            ExpireInMinutes = "60"
        };
        authService.LoginAsync(Arg.Any<LoginRequest>())
               .Returns(Task.FromResult(expectedResponse));

        var controller = new AuthController(authService);
        var request = new LoginRequest
        {
            Login = "testuser",
            Senha = "testpassword"
        };

        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task Login_CallsAuthServiceLoginAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new LoginRequest
        {
            Login = "testuser",
            Senha = "testpassword"
        };

        await controller.Login(request);

        await authService.Received(1).LoginAsync(request);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new LoginResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpireInMinutes = "60"
        };
        authService.RefreshAsync(Arg.Any<RefreshTokenRequest>())
               .Returns(Task.FromResult(expectedResponse));

        var controller = new AuthController(authService);
        var request = new RefreshTokenRequest
        {
            RefreshToken = "old_refresh_token"
        };

        var result = await controller.RefreshTokenAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task RefreshTokenAsync_CallsAuthServiceRefreshAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new RefreshTokenRequest
        {
            RefreshToken = "old_refresh_token"
        };

        await controller.RefreshTokenAsync(request);

        await authService.Received(1).RefreshAsync(request);
    }

    [Fact]
    public async Task LogoutAsync_ReturnsNoContent()
    {
        var authService = Substitute.For<IAuthService>();
        authService.LogoutAsync(Arg.Any<LogoutRequest>())
               .Returns(Task.CompletedTask);

        var controller = new AuthController(authService);
        var request = new LogoutRequest
        {
            RefreshToken = "test_refresh_token"
        };

        var result = await controller.LogoutAsync(request);

        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task LogoutAsync_CallsAuthServiceLogoutAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new LogoutRequest
        {
            RefreshToken = "test_refresh_token"
        };

        await controller.LogoutAsync(request);

        await authService.Received(1).LogoutAsync(request);
    }

    [Fact]
    public async Task Solicitar_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new Portal.Features.Auth.Domain.Responses.RecuperarSenhaResponse { EmailEnviado = true };
        authService.SolicitarRecuperacaoAsync(Arg.Any<Portal.Features.Auth.Domain.Requests.RecuperarSenhaRequest>())
            .Returns(expectedResponse);
        var controller = new AuthController(authService);
        var request = new Portal.Features.Auth.Domain.Requests.RecuperarSenhaRequest { Login = "testuser" };
        var result = await controller.Solicitar(request);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ValidarToken_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new ValidarTokenRecuperacaoResponse { Mensagem = "Token válido" };
        authService.ValidarTokenAsync(Arg.Any<ValidarTokenRecuperacaoRequest>())
            .Returns(expectedResponse);

        var controller = new AuthController(authService);
        var result = await controller.ValidarToken("sample-token");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
        await authService.Received(1).ValidarTokenAsync(
            Arg.Is<ValidarTokenRecuperacaoRequest>(request => request.Token == "sample-token"));
    }

    [Fact]
    public async Task TrocarSenha_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new TrocarSenhaResponse { Mensagem = "Senha alterada" };
        var request = new TrocarSenhaRequest
        {
            Token = "token",
            NovaSenha = "nova@123",
            ConfirmarSenha = "nova@123"
        };
        authService.TrocarSenhaAsync(Arg.Any<TrocarSenhaRequest>())
            .Returns(expectedResponse);

        var controller = new AuthController(authService);
        var result = await controller.TrocarSenha(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
        await authService.Received(1).TrocarSenhaAsync(request);
    }
}
