using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Perfil.UseCases.ApagarPerfil;
using Portal.Application.Perfil.UseCases.AtualizarNomePerfil;
using Portal.Application.Perfil.UseCases.ClonarPerfil;
using Portal.Application.Perfil.UseCases.CriarPerfil;
using Portal.Application.Perfil.UseCases.ObterPerfilPorId;
using Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;
using Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;
using Portal.Application.Perfil.UseCases.VincularEscoposPerfil;

namespace Portal.API.Endpoints;

public static class PerfilEndpoints
{
    public static void MapPerfilEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/perfis")
            .WithTags("🧩 Perfis");

        group.MapGet("/", async ([FromServices] ObterPerfisComEscopoHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterPerfisComEscopoRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/combo", async ([FromServices] ObterPerfisParaComboHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterPerfisParaComboRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/{id:int}", async ([FromServices] ObterPerfilPorIdHandler handler, int id, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterPerfilPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/", async ([FromServices] CriarPerfilHandler handler, [FromBody] CriarPerfilRequest request, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult($"/api/v1/perfis/{result.Data?.Id}");
        });

        group.MapPost("/{id:int}/clonar", async ([FromServices] ClonarPerfilHandler handler, int id, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ClonarPerfilRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapDelete("/{id:int}", async ([FromServices] ApagarPerfilHandler handler, int id, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ApagarPerfilRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/{id:int}/escopos", async (
            [FromServices] VincularEscoposPerfilHandler handler,
            int id,
            [FromBody] List<int> escopoIds,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new VincularEscoposPerfilRequest { PerfilId = id, EscopoIds = escopoIds }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPut("/{id:int}", async (
            [FromServices] AtualizarNomePerfilHandler handler,
            int id,
            [FromBody] string nome,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new AtualizarNomePerfilRequest { Id = id, NovoNome = nome }, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
