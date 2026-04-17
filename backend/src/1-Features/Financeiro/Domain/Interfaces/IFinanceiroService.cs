using Portal.Domain.Base;

namespace Portal.Features.Financeiro.Domain.Interfaces
{
    public interface IFinanceiroService
    {
        Task<Result<IEnumerable<FinanceiroResponse>>> ObterTodosLancamentosAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<FinanceiroResponse>>> ObterLancamentosPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken);
    }
}