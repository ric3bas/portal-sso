using Portal.Domain.Locacao;

namespace Portal.Domain.Locacao.Interfaces;

public interface ILocacaoRepository
{
    Task<IEnumerable<LocacaoQuery>> ObterTodasAsync(CancellationToken cancellationToken);
    Task<IEnumerable<LocacaoQuery>> ObterPorFiltroAsync(Guid? clienteId, Guid? equipamentoId, StatusLocacao? status, DateTime? dataRetiradaInicio, DateTime? dataRetiradaFim, CancellationToken cancellationToken);
    Task<LocacaoQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CriarAsync(LocacaoCommand locacao, CancellationToken cancellationToken);
    Task<int> AtualizarAsync(LocacaoCommand locacao, CancellationToken cancellationToken);
    Task<int> DevolverAsync(Guid id, DateTime dataDevolucao, string? observacao, CancellationToken cancellationToken);
    Task<int> CancelarAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ClienteExisteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<bool> EquipamentoExisteAsync(Guid equipamentoId, CancellationToken cancellationToken);
    Task<bool> ClienteBloqueadoAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<bool> EquipamentoDisponivelAsync(Guid equipamentoId, DateTime dataRetirada, DateTime previsaoDevolucao, Guid? locacaoIdIgnorar = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<LocacaoQuery>> ObterAtrasadasAsync(CancellationToken cancellationToken);
    Task AtualizarStatusAtrasadasAsync(CancellationToken cancellationToken);
    Task<decimal> ObterValorDiariaEquipamentoAsync(Guid equipamentoId, CancellationToken cancellationToken);
}
