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

namespace Portal.API.Endpoints;

public static class LocacaoEndpoints
{
    public static void MapLocacaoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/locacoes")
            .WithTags("📦 Locações");

        group.MapGet("/", async ([FromServices] ObterLocacoesHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterLocacoesRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/filtro", async (
            [FromServices] ObterLocacoesPorFiltroHandler handler,
            [FromQuery] Guid? clienteId,
            [FromQuery] Guid? equipamentoId,
            [FromQuery] StatusLocacao? status,
            [FromQuery] DateTime? dataRetiradaInicio,
            [FromQuery] DateTime? dataRetiradaFim,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterLocacoesPorFiltroRequest
            {
                ClienteId = clienteId,
                EquipamentoId = equipamentoId,
                Status = status,
                DataRetiradaInicio = dataRetiradaInicio,
                DataRetiradaFim = dataRetiradaFim
            };

            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/atrasadas", async ([FromServices] ObterLocacoesAtrasadasHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterLocacoesAtrasadasRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterLocacaoPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterLocacaoPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            [FromServices] CriarLocacaoHandler handler,
            [FromBody] CriarLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult($"/api/v1/locacoes/{result.Data?.Id}");
        });

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarLocacaoHandler handler,
            Guid id,
            [FromBody] AtualizarLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPatch("/{id:guid}/devolver", async (
            [FromServices] DevolverLocacaoHandler handler,
            Guid id,
            [FromBody] DevolverLocacaoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPatch("/{id:guid}/cancelar", async (
            [FromServices] CancelarLocacaoHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new CancelarLocacaoRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
