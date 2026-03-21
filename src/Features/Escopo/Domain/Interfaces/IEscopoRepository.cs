using EscopoEntity = Portal.Dominio.Entities.EscopoEntity;

namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoRepository
    {
        Task<IEnumerable<EscopoResponse>> ListarAsync(CancellationToken cancellationToken = default);
        Task<int> InserirAsync(EscopoEntity escopo, CancellationToken cancellationToken = default);
        Task<EscopoEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default);
    }
}
