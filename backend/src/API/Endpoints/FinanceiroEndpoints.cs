using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;
using Portal.WebApi.Extensions;

namespace Portal.API.Endpoints;

public static class FinanceiroEndpoints
{

    public static void MapFinanceiroEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/financeiro")
            .WithTags("💰 Financeiro");

        group.MapGet("/", async (
            [FromServices] ObterLancamentosFinanceirosHandler handler,
            [AsParameters] ObterLancamentosFinanceirosRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("financeiro.ler");

        group.MapGet("/periodo", async (
            [FromServices] ObterLancamentosFinanceirosPorPeriodoHandler handler,
            [AsParameters] ObterLancamentosFinanceirosPorPeriodoRequest request,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        })
        .RequireScopes("financeiro.ler");
    }
}
