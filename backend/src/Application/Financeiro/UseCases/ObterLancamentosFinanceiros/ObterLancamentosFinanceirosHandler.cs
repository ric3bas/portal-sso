using Portal.Domain.Base;
using Portal.Domain.Financeiro.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;

public class ObterLancamentosFinanceirosHandler
{
    private readonly IFinanceiroRepository _repository;

    public ObterLancamentosFinanceirosHandler(IFinanceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterLancamentosFinanceirosResponse>>> Handle(ObterLancamentosFinanceirosRequest request, CancellationToken cancellationToken)
    {
        var result = await _repository.ObterTodosAsync(cancellationToken);
        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterLancamentosFinanceirosResponse>>("Nenhum lançamento financeiro encontrado");

        return Result.OkResult(result.Select(x => x.ToResponse<ObterLancamentosFinanceirosResponse>()).ToList());
    }
}
