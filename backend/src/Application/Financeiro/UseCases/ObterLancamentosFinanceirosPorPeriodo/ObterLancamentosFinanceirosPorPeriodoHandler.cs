using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Financeiro.Interfaces;

namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;

public class ObterLancamentosFinanceirosPorPeriodoHandler
{
    private readonly IFinanceiroRepository _repository;

    public ObterLancamentosFinanceirosPorPeriodoHandler(IFinanceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterLancamentosFinanceirosPorPeriodoResponse>>> Handle(ObterLancamentosFinanceirosPorPeriodoRequest request, CancellationToken cancellationToken)
    {
        if (request.DataInicio > request.DataFim)
            return Result.ValidationResult<TabelaPaginadaResponse<ObterLancamentosFinanceirosPorPeriodoResponse>>("Data inÃ­cio deve ser anterior Ã  data fim");

        var resultado = await _repository.ObterPorPeriodoAsync(request.DataInicio.Date, request.DataFim.Date.AddDays(1).AddTicks(-1), request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterLancamentosFinanceirosPorPeriodoResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterLancamentosFinanceirosPorPeriodoResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
