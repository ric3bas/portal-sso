using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Cliente.UseCases.AtualizarCliente;
using Portal.Application.Cliente.UseCases.BloquearCliente;
using Portal.Application.Cliente.UseCases.CriarCliente;
using Portal.Application.Cliente.UseCases.DesbloquearCliente;
using Portal.Application.Cliente.UseCases.InativarCliente;
using Portal.Application.Cliente.UseCases.ObterClientePorId;
using Portal.Application.Cliente.UseCases.ObterClientes;
using Portal.Application.Cliente.UseCases.ObterClientesPorFiltro;

namespace Portal.API.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/clientes")
            .WithTags("👤 Clientes");

        group.MapGet("/", async (
            [FromServices] ObterClientesHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.Handle(new ObterClientesRequest(), cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("ListarClientes")
        .WithSummary("📋 Lista todos os clientes")
        .WithDescription("Retorna uma lista de todos os clientes disponíveis");

        group.MapGet("/filtro", async (
            [FromServices] ObterClientesPorFiltroHandler handler,
            [FromQuery] string? nome,
            [FromQuery] string? cpf,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var request = new ObterClientesPorFiltroRequest { Nome = nome, Cpf = cpf };
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("ListarClientesPorFiltro")
        .WithSummary("🔎 Lista clientes por filtro")
        .WithDescription("Retorna clientes por nome e/ou CPF");

        group.MapGet("/{id:guid}", async (
            [FromServices] ObterClientePorIdHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var request = new ObterClientePorIdRequest { Id = id };
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("ObterClientePorId")
        .WithSummary("🎯 Obtém cliente por ID")
        .WithDescription("Retorna um cliente específico pelo ID");

        group.MapPost("/", async (
            [FromServices] CriarClienteHandler handler,
            [FromBody] CriarClienteRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.Handle(request, cancellationToken);
                return result.ToCreatedResult($"/api/v1/clientes/{result.Data?.Id}");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("CriarCliente")
        .WithSummary("➕ Cria um novo cliente")
        .WithDescription("Cria um novo cliente no sistema");

        group.MapPatch("/{id:guid}", async (
            [FromServices] AtualizarClienteHandler handler,
            Guid id,
            [FromBody] AtualizarClienteRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                request.Id = id;
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("AtualizarCliente")
        .WithSummary("✏️ Atualiza cliente")
        .WithDescription("Atualiza os dados do cliente");

        group.MapPatch("/{id:guid}/bloquear", async (
            [FromServices] BloquearClienteHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var request = new BloquearClienteRequest { Id = id };
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("BloquearCliente")
        .WithSummary("🔒 Bloqueia cliente")
        .WithDescription("Bloqueia um cliente pelo ID");

        group.MapPatch("/{id:guid}/desbloquear", async (
            [FromServices] DesbloquearClienteHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var request = new DesbloquearClienteRequest { Id = id };
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("DesbloquearCliente")
        .WithSummary("🔓 Desbloqueia cliente")
        .WithDescription("Desbloqueia um cliente pelo ID");

        group.MapPatch("/{id:guid}/inativar", async (
            [FromServices] InativarClienteHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var request = new InativarClienteRequest { Id = id };
                var result = await handler.Handle(request, cancellationToken);
                return result.ToHttpResult();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro interno do servidor");
            }
        })
        .WithName("InativarCliente")
        .WithSummary("🚫 Inativa cliente")
        .WithDescription("Inativa um cliente pelo ID");
    }
}
