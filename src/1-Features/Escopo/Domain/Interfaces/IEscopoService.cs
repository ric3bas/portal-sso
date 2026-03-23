using Portal.Domain.Base;
using Portal.Features.Escopo.Infra;

namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoService
    {
        Task<Result<IEnumerable<EscopoResponse>>> ListarAsync(CancellationToken cancellationToken = default);
        Task<Result<string>> CriarAsync(string nome, CancellationToken cancellationToken = default);
        Task<Result<EscopoResponse?>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
