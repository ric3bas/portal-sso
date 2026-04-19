using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Usuario.UseCases.AtualizarUsuario;
using Portal.Application.Usuario.UseCases.ListarUsuarios;
using Portal.Application.Usuario.UseCases.RegistrarUsuario;

namespace Portal.API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/usuarios")
            .WithTags("👥 Usuários");

        group.MapGet("/", async (
            [FromServices] ListarUsuariosHandler handler,
            [FromQuery] string? parceiroId,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListarUsuariosRequest { ParceiroId = parceiroId }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            [FromServices] RegistrarUsuarioHandler handler,
            [FromBody] RegistrarUsuarioRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPatch("/{id:int}", async (
            [FromServices] AtualizarUsuarioHandler handler,
            int id,
            [FromBody] AtualizarUsuarioRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
