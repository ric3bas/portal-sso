using Portal.Domain.Base;

namespace Portal.Features.Perfil.Domain.Interfaces
{
    public interface IPerfilService
    {
        Task<Result<IEnumerable<PerfilComEscopoResponse>>> ListarComEscoposAsync(CancellationToken cancellationToken = default);
        Task<Result<string>> CriarAsync(string nome, CancellationToken cancellationToken = default);
        Task<Result<PerfilResponse?>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<string>> VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default);
    }
}
