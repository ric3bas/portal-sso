using Microsoft.AspNetCore.Mvc;
using Portal.API.Extensions;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;

namespace Portal.API.Endpoints;

public static class FinanceiroEndpoints
{
    public static void MapFinanceiroEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/financeiro")
            .WithTags("💰 Financeiro");

        group.MapGet("/", async (
            [FromServices] ObterLancamentosFinanceirosHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ObterLancamentosFinanceirosRequest(), cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/periodo", async (
            [FromServices] ObterLancamentosFinanceirosPorPeriodoHandler handler,
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterLancamentosFinanceirosPorPeriodoRequest
            {
                DataInicio = dataInicio,
                DataFim = dataFim
            };

            var result = await handler.Handle(request, cancellationToken);
            return result.ToHttpResult();
        });
    }
}
