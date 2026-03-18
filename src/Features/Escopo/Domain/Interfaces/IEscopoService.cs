using EscopoEntity = Portal.Dominio.Entities.Escopo;

namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoService
    {
        Task<IEnumerable<EscopoResponse>> ListarAsync(CancellationToken cancellationToken = default);
        Task<int> CriarAsync(string nome, CancellationToken cancellationToken = default);
        Task<EscopoEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
