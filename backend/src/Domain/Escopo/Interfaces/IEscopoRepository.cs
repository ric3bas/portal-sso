using Portal.Domain.Escopo;

namespace Portal.Domain.Escopo.Interfaces;

public interface IEscopoRepository
{
    Task<IEnumerable<EscopoQuery>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<EscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<int> CriarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
    Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null, CancellationToken cancellationToken = default);
}
