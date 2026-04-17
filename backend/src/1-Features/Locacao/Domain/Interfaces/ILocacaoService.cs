using Portal.Domain.Base;

namespace Portal.Features.Locacao.Domain.Interfaces
{
    public interface ILocacaoService
    {
        Task<Result<IEnumerable<LocacaoResponse>>> ObterTodasLocacoesAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<LocacaoResponse>>> ObterLocacoesPorFiltroAsync(FiltroLocacaoRequest filtro, CancellationToken cancellationToken);
        Task<Result<LocacaoResponse>> ObterLocacaoAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> CriarLocacaoAsync(LocacaoRequest locacao, CancellationToken cancellationToken);
        Task<Result<string>> AtualizarLocacaoAsync(AtualizarLocacaoRequest locacao, CancellationToken cancellationToken);
        Task<Result<string>> DevolverLocacaoAsync(DevolverLocacaoRequest devolucao, CancellationToken cancellationToken);
        Task<Result<string>> CancelarLocacaoAsync(string? id, CancellationToken cancellationToken);
        Task<Result<IEnumerable<LocacaoResponse>>> ObterLocacoesAtrasadasAsync(CancellationToken cancellationToken);
    }
}