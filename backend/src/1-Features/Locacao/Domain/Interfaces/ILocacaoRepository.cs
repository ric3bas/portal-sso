using Portal.Features.Locacao.Domain;

namespace Portal.Features.Locacao.Domain.Interfaces
{
    public interface ILocacaoRepository
    {
        Task<IEnumerable<LocacaoEntity>> ObterTodasAsync(CancellationToken cancellationToken);
        Task<IEnumerable<LocacaoEntity>> ObterPorFiltroAsync(FiltroLocacaoRequest filtro, CancellationToken cancellationToken);
        Task<LocacaoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(LocacaoRequest locacao, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(Guid id, AtualizarLocacaoRequest locacao, CancellationToken cancellationToken);
        Task<int> DevolverAsync(Guid id, DateTime dataDevolucao, string? observacao, CancellationToken cancellationToken);
        Task<int> CancelarAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ClienteExisteAsync(Guid clienteId, CancellationToken cancellationToken);
        Task<bool> EquipamentoExisteAsync(Guid equipamentoId, CancellationToken cancellationToken);
        Task<bool> ClienteBloqueadoAsync(Guid clienteId, CancellationToken cancellationToken);
        Task<bool> EquipamentoDisponivelAsync(Guid equipamentoId, DateTime dataRetirada, DateTime previsaoDevolucao, Guid? locacaoIdIgnorar = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<LocacaoEntity>> ObterAtrasadasAsync(CancellationToken cancellationToken);
        Task AtualizarStatusAtrasadasAsync(CancellationToken cancellationToken);
        Task<decimal> ObterValorDiariaEquipamentoAsync(Guid equipamentoId, CancellationToken cancellationToken);
    }
}