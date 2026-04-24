using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Escopo.UseCases.AtualizarEscopo;
using Portal.Application.Escopo.UseCases.CriarEscopo;
using Portal.Application.Escopo.UseCases.ObterEscopoPorId;
using Portal.Application.Escopo.UseCases.ObterEscopos;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class EscopoEndpoints
{
    public static void MapEscopoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/escopos")
            .WithTags("🔐 Escopos");

        group.MapGet("/", async ([FromServices] ObterEscoposHandler handler, [AsParameters] ObterEscoposRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("escopo.ler");

        group.MapGet("/{id:int}", async (
            [FromServices] ObterEscopoPorIdHandler handler,
            int id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterEscopoPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("escopo.ler");

        group.MapPost("/", async (
            [FromServices] CriarEscopoHandler handler,
            [FromBody] CriarEscopoRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult(result.Data);
        })
        .RequireScopes("escopo.criar");

        group.MapPut("/{id:int}", async (
            [FromServices] AtualizarEscopoHandler handler,
            int id,
            [FromBody] AtualizarEscopoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("escopo.atualizar");
    }
}
