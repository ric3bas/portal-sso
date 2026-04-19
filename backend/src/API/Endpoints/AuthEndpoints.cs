using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Auth.UseCases.Login;
using Portal.Application.Auth.UseCases.Logout;
using Portal.Application.Auth.UseCases.RecuperarSenha;
using Portal.Application.Auth.UseCases.RefreshToken;
using Portal.Application.Auth.UseCases.TrocarSenha;
using Portal.Application.Auth.UseCases.ValidarTokenRecuperacao;

namespace Portal.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("🔑 Auth");

        group.MapPost("/login", async (
            [FromServices] LoginHandler handler,
            HttpContext httpContext,
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken) =>
        {
            request.IpUsuario = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/refresh-token", async (
            [FromServices] RefreshTokenHandler handler,
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/logout", async (
            [FromServices] LogoutHandler handler,
            [FromBody] LogoutRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/esqueceu-senha", async (
            [FromServices] RecuperarSenhaHandler handler,
            [FromBody] RecuperarSenhaRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/esqueceu-senha-logado", async (
            [FromServices] RecuperarSenhaHandler handler,
            [FromBody] RecuperarSenhaRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleLogado(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/validar-token", async (
            [FromServices] ValidarTokenRecuperacaoHandler handler,
            [FromQuery] string token,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ValidarTokenRecuperacaoRequest { Token = token }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/trocar-senha", async (
            [FromServices] TrocarSenhaHandler handler,
            [FromBody] TrocarSenhaRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
