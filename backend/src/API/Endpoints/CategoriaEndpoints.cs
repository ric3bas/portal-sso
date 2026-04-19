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

        // GET /api/v1/categorias - Lista todas as categorias
        group.MapGet("/", async (
            [FromServices] ObterCategoriasHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterCategoriasRequest();
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterTodasCategorias")
        .WithSummary("📋 Lista todas as categorias")
        .WithDescription("Retorna uma lista de todas as categorias disponíveis para o usuário autenticado. Para usuários master, retorna todas as categorias do sistema.")
        .Produces<Portal.Application.Categoria.UseCases.ObterCategorias.ObterCategoriasResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // GET /api/v1/categorias/filtro - Lista categorias por filtro de nome
        group.MapGet("/filtro", async (
            [FromServices] ObterCategoriasPorFiltroHandler handler,
            [FromQuery] string? nome,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterCategoriasPorFiltroRequest { Nome = nome };
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterCategoriasPorFiltro")
        .WithSummary("🔍 Busca categorias por nome")
        .WithDescription("Retorna uma lista de categorias filtradas pelo nome informado. O filtro é case-insensitive e busca por correspondência parcial.")
        .Produces<Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro.ObterCategoriasPorFiltroResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // GET /api/v1/categorias/id - Retorna uma categoria pelo Id (query parameter)
        group.MapGet("/id", async (
            [FromServices] ObterCategoriaPorIdHandler handler,
            [FromQuery] string? id,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guidId))
            {
                return Results.BadRequest(new { errors = new[] { "ID inválido" } });
            }

            var request = new ObterCategoriaPorIdRequest { Id = guidId };
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ObterCategoriaPorIdQuery")
        .WithSummary("🎯 Obtém categoria por ID (via query)")
        .WithDescription("Retorna uma categoria específica pelo seu ID informado como query parameter.")
        .RequireScopes("categoria.ler")
        .Produces<Portal.Application.Categoria.UseCases.ObterCategoriaPorId.ObterCategoriaPorIdResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // GET /api/v1/categorias/{id} - Retorna uma categoria pelo Id (route parameter)
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

        // POST /api/v1/categorias - Cria uma nova categoria
        group.MapPost("/", async (
            [FromServices] CriarCategoriaHandler handler,
            [FromBody] CriarCategoriaRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToCreatedResult($"/api/v1/categorias/{result.Data?.Id}");
        })
        .WithName("CriarCategoria")
        .WithSummary("➕ Cria uma nova categoria")
        .WithDescription("Cria uma nova categoria no sistema. O nome deve ser único dentro do escopo do parceiro.")
        .RequireScopes("categoria.criar")
        .Accepts<CriarCategoriaRequest>("application/json")
        .Produces<Portal.Application.Categoria.UseCases.CriarCategoria.CriarCategoriaResponse>(201, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(409);

        // PATCH /api/v1/categorias/id - Atualiza uma categoria existente (query parameter)
        group.MapPatch("/id", async (
            [FromServices] AtualizarCategoriaHandler handler,
            [FromQuery] string? id,
            [FromBody] AtualizarCategoriaRequest request,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guidId))
            {
                return Results.BadRequest(new { errors = new[] { "ID inválido" } });
            }

            request.Id = guidId;
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("AtualizarCategoriaQuery")
        .WithSummary("✏️ Atualiza categoria (via query)")
        .WithDescription("Atualiza uma categoria existente usando o ID como query parameter.")
        .RequireScopes("categoria.atualizar")
        .Accepts<AtualizarCategoriaRequest>("application/json")
        .Produces<Portal.Application.Categoria.UseCases.AtualizarCategoria.AtualizarCategoriaResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409);

        // PATCH /api/v1/categorias/{id} - Atualiza uma categoria existente (route parameter)
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
        .Produces<Portal.Application.Categoria.UseCases.AtualizarCategoria.AtualizarCategoriaResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(409);

        // PATCH /api/v1/categorias/{id}/inativar - Inativa uma categoria
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
        .Produces<Portal.Application.Categoria.UseCases.InativarCategoria.InativarCategoriaResponse>(200, "application/json")
        .ProducesValidationProblem(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);
    }
}
