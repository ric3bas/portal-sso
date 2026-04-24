using Portal.Domain.Escopo;
using Portal.Domain.Common;

namespace Portal.Domain.Escopo.Interfaces;

public interface IEscopoRepository
{
    Task<ResultadoPaginado<EscopoQuery>> ObterTodosAsync(string? nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task<EscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<int> CriarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
    Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null, CancellationToken cancellationToken = default);
}
