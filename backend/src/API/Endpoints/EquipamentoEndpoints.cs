using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Equipamento.UseCases.AtualizarEquipamento;
using Portal.Application.Equipamento.UseCases.CriarEquipamento;
using Portal.Application.Equipamento.UseCases.InativarEquipamento;
using Portal.Application.Equipamento.UseCases.ObterEquipamentoPorId;
using Portal.Application.Equipamento.UseCases.ObterEquipamentos;
using Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class EquipamentoEndpoints
{

    public static void MapEquipamentoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/equipamentos")
            .WithTags("🧰 Equipamentos");

        group.MapGet("/", async ([FromServices] ObterEquipamentosHandler handler, [AsParameters] ObterEquipamentosRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("equipamento.ler");

        group.MapGet("/filtro", async (
            [FromServices] ObterEquipamentosPorFiltroHandler handler,
            [AsParameters] ObterEquipamentosPorFiltroRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("equipamento.ler");

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterEquipamentoPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterEquipamentoPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("equipamento.ler");

        group.MapPost("/", async (
            [FromServices] CriarEquipamentoHandler handler,
            [FromBody] CriarEquipamentoRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult(result.Data);
        })
        .RequireScopes("equipamento.criar");

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarEquipamentoHandler handler,
            Guid id,
            [FromBody] AtualizarEquipamentoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("equipamento.atualizar");

        group.MapPatch("/{id:guid}/inativar", async (
            [FromServices] InativarEquipamentoHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new InativarEquipamentoRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("equipamento.inativar");
    }
}
