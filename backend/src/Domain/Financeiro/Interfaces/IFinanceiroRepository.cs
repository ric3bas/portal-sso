using Portal.Domain.Financeiro;
using Portal.Domain.Common;

namespace Portal.Domain.Financeiro.Interfaces;

public interface IFinanceiroRepository
{
    Task<Guid> CriarLancamentoAsync(FinanceiroCommand lancamento, CancellationToken cancellationToken);
    Task<ResultadoPaginado<FinanceiroQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<ResultadoPaginado<FinanceiroQuery>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<bool> ExisteLancamentoParaLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken);
}
