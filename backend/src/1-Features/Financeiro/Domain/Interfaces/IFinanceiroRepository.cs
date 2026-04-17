using Portal.Features.Financeiro.Domain;

namespace Portal.Features.Financeiro.Domain.Interfaces
{
    public interface IFinanceiroRepository
    {
        Task<Guid> CriarLancamentoAsync(Guid locacaoId, DateTime dataDevolucao, CancellationToken cancellationToken);
        Task<IEnumerable<FinanceiroEntity>> ObterTodosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<FinanceiroEntity>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken);
        Task<bool> ExisteLancamentoParaLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken);
    }
}