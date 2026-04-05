using Portal.Domain.Base;
using Portal.Features.Escopo.Infra;

namespace Portal.Features.Escopo.Domain.Interfaces
{
    public interface IEscopoService
    {
        Task<Result<IEnumerable<EscopoResponse>>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<Result<string>> CriarAsync(EscopoRequest request, CancellationToken cancellationToken = default);
        Task<Result<EscopoResponse?>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<string>> AtualizarAsync(AtualizarEscopoRequest nome, CancellationToken cancellationToken = default);
    }
}
