using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Parceiro.UseCases.AtualizarParceiro;
using Portal.Application.Parceiro.UseCases.CriarParceiro;
using Portal.Application.Parceiro.UseCases.ObterParceiroPorId;
using Portal.Application.Parceiro.UseCases.ObterParceiros;
using Portal.Application.Parceiro.UseCases.ObterParceirosPorFiltro;

namespace Portal.API.Endpoints;

public static class ParceiroEndpoints
{
    public static void MapParceiroEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/parceiros")
            .WithTags("🤝 Parceiros");

        group.MapGet("/", async ([FromServices] ObterParceirosHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterParceirosRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/filtro", async (
            [FromServices] ObterParceirosPorFiltroHandler handler,
            [FromQuery] string? nome,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterParceirosPorFiltroRequest { Nome = nome ?? string.Empty }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterParceiroPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterParceiroPorIdRequest { Id = id }, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            [FromServices] CriarParceiroHandler handler,
            [FromBody] CriarParceiroRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult($"/api/v1/parceiros/{result.Data?.Id}");
        });

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarParceiroHandler handler,
            Guid id,
            [FromBody] AtualizarParceiroRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
