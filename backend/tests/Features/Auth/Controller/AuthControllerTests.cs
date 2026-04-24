using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Controller;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Domain.Responses;

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
    [InlineData("LoginAsync", typeof(HttpPostAttribute))]
    [InlineData("RefreshTokenAsync", typeof(HttpPostAttribute))]
    [InlineData("LogoutAsync", typeof(HttpPostAttribute))]
    [InlineData("SolicitarAsync", typeof(HttpPostAttribute))]
    [InlineData("ValidarTokenAsync", typeof(HttpGetAttribute))]
    public void Endpoint_Exists(string methodName, Type httpAttribute)
    {
        var method = _controller.GetMethod(methodName);
        Assert.NotNull(method);
        Assert.True(method.GetCustomAttributes(httpAttribute, false).Length > 0);
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
        authService.RefreshAsync(Arg.Any<RefreshTokenRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResponse));
        var controller = new AuthController(authService);
        var request = new RefreshTokenRequest
        {
            RefreshToken = "old_refresh_token"
        };
        var result = await controller.RefreshTokenAsync(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
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
        await controller.RefreshTokenAsync(request, CancellationToken.None);
        await authService.Received(1).RefreshAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_ReturnsNoContent()
    {
        var authService = Substitute.For<IAuthService>();
        authService.LogoutAsync(Arg.Any<LogoutRequest>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var controller = new AuthController(authService);
        var request = new LogoutRequest
        {
            RefreshToken = "test_refresh_token"
        };
        var result = await controller.LogoutAsync(request, CancellationToken.None);
        var noContentResult = Assert.IsType<NoContentResult>(result.Data);
        Assert.Equal(204, noContentresult.Data.StatusCode);
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
        await controller.LogoutAsync(request, CancellationToken.None);
        await authService.Received(1).LogoutAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new LoginResponse
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpireInMinutes = "60"
        };
        authService.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResponse));
        var controller = new AuthController(authService);
        var request = new LoginRequest
        {
            Login = "testuser",
            Senha = "password123"
        };
        var result = await controller.LoginAsync(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task LoginAsync_CallsAuthServiceLoginAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new LoginRequest
        {
            Login = "testuser",
            Senha = "password123"
        };
        await controller.LoginAsync(request, CancellationToken.None);
        await authService.Received(1).LoginAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SolicitarAsync_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new RecuperarSenhaResponse
        {
            EmailEnviado = true
        };
        authService.SolicitarRecuperacaoAsync(Arg.Any<RecuperarSenhaRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResponse));
        var controller = new AuthController(authService);
        var request = new RecuperarSenhaRequest
        {
            Login = "testuser"
        };
        var result = await controller.SolicitarAsync(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task SolicitarAsync_CallsAuthServiceSolicitarRecuperacaoAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new RecuperarSenhaRequest
        {
            Login = "testuser"
        };
        await controller.SolicitarAsync(request, CancellationToken.None);
        await authService.Received(1).SolicitarRecuperacaoAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidarTokenAsync_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new ValidarTokenRecuperacaoResponse
        {
            Mensagem = "Token válido"
        };
        authService.ValidarTokenAsync(Arg.Any<ValidarTokenRecuperacaoRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResponse));
        var controller = new AuthController(authService);
        var token = "test_token_123";
        var result = await controller.ValidarTokenAsync(token, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ValidarTokenAsync_CallsAuthServiceValidarTokenAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var token = "test_token_123";
        await controller.ValidarTokenAsync(token, CancellationToken.None);
        await authService.Received(1).ValidarTokenAsync(Arg.Is<ValidarTokenRecuperacaoRequest>(r => r.Token == token), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TrocarSenhaAsync_ReturnsOkWithResponse()
    {
        var authService = Substitute.For<IAuthService>();
        var expectedResponse = new TrocarSenhaResponse
        {
            Mensagem = "Senha alterada com sucesso"
        };
        authService.TrocarSenhaAsync(Arg.Any<TrocarSenhaRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResponse));
        var controller = new AuthController(authService);
        var request = new TrocarSenhaRequest
        {
            Token = "test_token",
            NovaSenha = "newpassword123"
        };
        var result = await controller.TrocarSenhaAsync(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task TrocarSenhaAsync_CallsAuthServiceTrocarSenhaAsync()
    {
        var authService = Substitute.For<IAuthService>();
        var controller = new AuthController(authService);
        var request = new TrocarSenhaRequest
        {
            Token = "test_token",
            NovaSenha = "newpassword123"
        };
        await controller.TrocarSenhaAsync(request, CancellationToken.None);
        await authService.Received(1).TrocarSenhaAsync(request, Arg.Any<CancellationToken>());
    }

}
