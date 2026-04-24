using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Usuario.UseCases.AtualizarUsuario;
using Portal.Application.Usuario.UseCases.ObterUsuarios;
using Portal.Application.Usuario.UseCases.CriarUsuario;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/usuarios")
            .WithTags("👥 Usuários");

        group.MapGet("/", async (
            [FromServices] ObterUsuariosHandler handler,
            [AsParameters] ObterUsuariosRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("usuario.ler");

        group.MapPost("/", async (
            [FromServices] CriarUsuarioHandler handler,
            [FromBody] CriarUsuarioRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("usuario.criar");

        group.MapPatch("/{id:int}", async (
            [FromServices] AtualizarUsuarioHandler handler,
            int id,
            [FromBody] AtualizarUsuarioRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("usuario.atualizar");
    }
}

