using Portal.Domain.Financeiro;

namespace Portal.Domain.Financeiro.Interfaces;

public interface IFinanceiroRepository
{
    Task<Guid> CriarLancamentoAsync(FinanceiroCommand lancamento, CancellationToken cancellationToken);
    Task<IEnumerable<FinanceiroQuery>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<FinanceiroQuery>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken);
    Task<bool> ExisteLancamentoParaLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken);
}
