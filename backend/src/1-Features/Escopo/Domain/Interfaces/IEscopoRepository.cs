using Portal.Features.Escopo.Infra;
namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoRepository
    {
        Task<IEnumerable<EscopoQuery>> ListarAsync(CancellationToken cancellationToken = default);
        Task<int> InserirAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
        Task<EscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default);
        Task AtualizarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default);
    }
}
