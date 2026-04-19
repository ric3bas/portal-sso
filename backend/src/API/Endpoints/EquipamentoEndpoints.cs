using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Equipamento.UseCases.AtualizarEquipamento;
using Portal.Application.Equipamento.UseCases.CriarEquipamento;
using Portal.Application.Equipamento.UseCases.InativarEquipamento;
using Portal.Application.Equipamento.UseCases.ObterEquipamentoPorId;
using Portal.Application.Equipamento.UseCases.ObterEquipamentos;
using Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;

namespace Portal.API.Endpoints;

public static class EquipamentoEndpoints
{
    public static void MapEquipamentoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/equipamentos")
            .WithTags("🧰 Equipamentos");

        group.MapGet("/", async ([FromServices] ObterEquipamentosHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterEquipamentosRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/filtro", async (
            [FromServices] ObterEquipamentosPorFiltroHandler handler,
            [FromQuery] string? nome,
            [FromQuery] string? marca,
            [FromQuery] string? modelo,
            [FromQuery] Guid? categoriaId,
            [FromQuery] bool? ativo,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterEquipamentosPorFiltroRequest
            {
                Nome = nome,
                Marca = marca,
                Modelo = modelo,
                CategoriaId = categoriaId,
                Ativo = ativo
            };

            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterEquipamentoPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterEquipamentoPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            [FromServices] CriarEquipamentoHandler handler,
            [FromBody] CriarEquipamentoRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult($"/api/v1/equipamentos/{result.Data?.Id}");
        });

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarEquipamentoHandler handler,
            Guid id,
            [FromBody] AtualizarEquipamentoRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPatch("/{id:guid}/inativar", async (
            [FromServices] InativarEquipamentoHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new InativarEquipamentoRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
