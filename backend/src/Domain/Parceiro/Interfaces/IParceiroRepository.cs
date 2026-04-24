using Portal.Domain.Common;

namespace Portal.Domain.Parceiro.Interfaces;

public interface IParceiroRepository
{
    Task<ResultadoPaginado<ParceiroQuery>> ObterTodosAsync(Guid? id, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<ParceiroQuery>> ObterTodosPorFiltroAsync(string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task<ParceiroQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParceiroQuery?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<Guid> InserirAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default);
    Task AtualizarAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default);
    Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default);
}
