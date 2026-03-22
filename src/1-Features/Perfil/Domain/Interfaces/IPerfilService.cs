

namespace Portal.Features.Perfil.Domain.Interfaces
{
    public interface IPerfilService
    {
        Task<IEnumerable<PerfilComEscopoResponse>> ListarComEscoposAsync(CancellationToken cancellationToken = default);
        Task<int> CriarAsync(string nome, CancellationToken cancellationToken = default);
        Task<PerfilResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default);
    }
}
