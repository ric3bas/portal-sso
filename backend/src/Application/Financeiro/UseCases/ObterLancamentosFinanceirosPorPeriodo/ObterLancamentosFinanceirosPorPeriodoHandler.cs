using Portal.Domain.Base;
using Portal.Domain.Financeiro.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;

public class ObterLancamentosFinanceirosPorPeriodoHandler
{
    private readonly IFinanceiroRepository _repository;

    public ObterLancamentosFinanceirosPorPeriodoHandler(IFinanceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterLancamentosFinanceirosPorPeriodoResponse>>> Handle(ObterLancamentosFinanceirosPorPeriodoRequest request, CancellationToken cancellationToken)
    {
        if (request.DataInicio > request.DataFim)
            return Result.ValidationResult<List<ObterLancamentosFinanceirosPorPeriodoResponse>>("Data início deve ser anterior à data fim");

        var result = await _repository.ObterPorPeriodoAsync(request.DataInicio.Date, request.DataFim.Date.AddDays(1).AddTicks(-1), cancellationToken);
        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterLancamentosFinanceirosPorPeriodoResponse>>("Nenhum lançamento financeiro encontrado no período");

        return Result.OkResult(result.Select(x => x.ToResponse<ObterLancamentosFinanceirosPorPeriodoResponse>()).ToList());
    }
}
