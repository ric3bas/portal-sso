using Portal.Domain.Locacao;
using Portal.Domain.Common;

namespace Portal.Domain.Locacao.Interfaces;

public interface ILocacaoRepository
{
    Task<ResultadoPaginado<LocacaoQuery>> ObterTodasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<ResultadoPaginado<LocacaoQuery>> ObterPorFiltroAsync(Guid? clienteId, Guid? equipamentoId, StatusLocacao? status, DateTime? dataRetiradaInicio, DateTime? dataRetiradaFim, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
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
    Task<ResultadoPaginado<LocacaoQuery>> ObterAtrasadasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task AtualizarStatusAtrasadasAsync(CancellationToken cancellationToken);
    Task<decimal> ObterValorDiariaEquipamentoAsync(Guid equipamentoId, CancellationToken cancellationToken);
}
