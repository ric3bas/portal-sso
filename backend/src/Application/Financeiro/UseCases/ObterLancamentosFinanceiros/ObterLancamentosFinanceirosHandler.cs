using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Financeiro.Interfaces;

namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;

public class ObterLancamentosFinanceirosHandler
{
    private readonly IFinanceiroRepository _repository;

    public ObterLancamentosFinanceirosHandler(IFinanceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterLancamentosFinanceirosResponse>>> Handle(ObterLancamentosFinanceirosRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterTodosAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterLancamentosFinanceirosResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterLancamentosFinanceirosResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
