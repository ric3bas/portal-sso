using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Locacao.UseCases.AtualizarLocacao;
using Portal.Application.Locacao.UseCases.CancelarLocacao;
using Portal.Application.Locacao.UseCases.CriarLocacao;
using Portal.Application.Locacao.UseCases.DevolverLocacao;
using Portal.Application.Locacao.UseCases.ObterLocacaoPorId;
using Portal.Application.Locacao.UseCases.ObterLocacoes;
using Portal.Application.Locacao.UseCases.ObterLocacoesAtrasadas;
using Portal.Application.Locacao.UseCases.ObterLocacoesPorFiltro;
using Portal.Domain.Locacao;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class LocacaoEndpoints
{

    public static void MapLocacaoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/locacoes")
            .WithTags("📦 Locações");

        group.MapGet("/", async ([FromServices] ObterLocacoesHandler handler, [AsParameters] ObterLocacoesRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.ler");

        group.MapGet("/filtro", async (
            [FromServices] ObterLocacoesPorFiltroHandler handler,
            [AsParameters] ObterLocacoesPorFiltroRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.ler");

        group.MapGet("/atrasadas", async ([FromServices] ObterLocacoesAtrasadasHandler handler, [AsParameters] ObterLocacoesAtrasadasRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.ler");

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterLocacaoPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterLocacaoPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.ler");

        group.MapPost("/", async (
            [FromServices] CriarLocacaoHandler handler,
            [FromBody] CriarLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult(result.Data);
        })
        .RequireScopes("locacao.criar");

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarLocacaoHandler handler,
            Guid id,
            [FromBody] AtualizarLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.atualizar");

        group.MapPatch("/{id:guid}/devolver", async (
            [FromServices] DevolverLocacaoHandler handler,
            Guid id,
            [FromBody] DevolverLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.devolver");

        group.MapPatch("/{id:guid}/cancelar", async (
            [FromServices] CancelarLocacaoHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new CancelarLocacaoRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("locacao.atualizar");
    }
}
