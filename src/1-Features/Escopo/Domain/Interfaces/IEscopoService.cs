using Portal.Features.Escopo.Infra;

namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoService
    {
        Task<IEnumerable<EscopoResponse>> ListarAsync(CancellationToken cancellationToken = default);
        Task<int> CriarAsync(string nome, CancellationToken cancellationToken = default);
        Task<EscopoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
