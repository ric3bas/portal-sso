using Microsoft.AspNetCore.Mvc;
using Portal.Application.Categoria.UseCases.CriarCategoria;
using Portal.Application.Categoria.UseCases.ObterCategorias;
using Portal.Application.Categoria.UseCases.ObterCategoriaPorId;
using Portal.Application.Categoria.UseCases.AtualizarCategoria;
using Portal.Application.Categoria.UseCases.InativarCategoria;
using Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro;
using Portal.API.Extensions;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class CategoriaEndpoints
{

    public static void MapCategoriaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categorias")
            .WithTags("🏷️ Categorias");

        group.MapGet("/", async (
            [FromServices] ObterCategoriasHandler handler,
            [AsParameters] ObterCategoriasRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterTodasCategorias")
        .WithSummary("📋 Lista todas as categorias")
        .WithDescription("Retorna uma lista de todas as categorias disponíveis para o usuário autenticado. Para usuários master, retorna todas as categorias do sistema.")
        .Produces<ObterCategoriasResponse>(200, "application/json")
        .RequireScopes("categoria.ler")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        group.MapGet("/filtro", async (
            [FromServices] ObterCategoriasPorFiltroHandler handler,
            [AsParameters] ObterCategoriasPorFiltroRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterCategoriasPorFiltro")
        .WithSummary("🔍 Busca categorias por nome")
        .WithDescription("Retorna uma lista de categorias filtradas pelo nome informado. O filtro é case-insensitive e busca por correspondência parcial.")
        .RequireScopes("categoria.ler")
        .Produces<Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro.ObterCategoriasPorFiltroResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterCategoriaPorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterCategoriaPorIdRequest { Id = id };
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterCategoriaPorIdRoute")
        .WithSummary("🎯 Obtém categoria por ID")
        .WithDescription("Retorna uma categoria específica pelo seu ID informado como parâmetro de rota.")
        .RequireScopes("categoria.ler")
        .Produces<Portal.Application.Categoria.UseCases.ObterCategoriaPorId.ObterCategoriaPorIdResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        group.MapPost("/", async (
            [FromServices] CriarCategoriaHandler handler,
            [FromBody] CriarCategoriaRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult(result.Data);
        })
        .WithName("CriarCategoria")
        .WithSummary("➕ Cria uma nova categoria")
        .WithDescription("Cria uma nova categoria no sistema. O nome deve ser único dentro do escopo do parceiro.")
        .RequireScopes("categoria.criar")
        .Accepts<CriarCategoriaRequest>("application/json")
        .Produces<string>(201, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(409);


        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarCategoriaHandler handler,
            Guid id,
            [FromBody] AtualizarCategoriaRequest request,
            CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("AtualizarCategoriaRoute")
        .WithSummary("✏️ Atualiza categoria")
        .WithDescription("Atualiza uma categoria existente. O nome deve ser único dentro do escopo do parceiro.")
        .RequireScopes("categoria.atualizar")
        .Accepts<AtualizarCategoriaRequest>("application/json")
        .Produces<string>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409);

        group.MapPatch("/{id}/inativar", async (
            [FromServices] InativarCategoriaHandler handler,
            string id,
            CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                return Results.BadRequest(new { errors = new[] { "ID inválido" } });
            }

            var request = new InativarCategoriaRequest { Id = guidId };
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("InativarCategoria")
        .WithSummary("🚫 Inativa uma categoria")
        .WithDescription("Marca uma categoria como inativa, impedindo seu uso em novas operações.")
        .RequireScopes("categoria.inativar")
        .Produces<string>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);
    }
}
