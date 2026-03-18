using PerfilEntity = Portal.Dominio.Entities.Perfil;

namespace Portal.Features.Perfil.Domain.Interfaces
{
    public interface IPerfilRepository
    {
        Task<IEnumerable<PerfilComEscopoResponse>> ListarComEscoposAsync(CancellationToken cancellationToken = default);
        Task<int> InserirAsync(PerfilEntity perfil, CancellationToken cancellationToken = default);
        Task<PerfilEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistePerfilAsync(int perfilId, CancellationToken cancellationToken = default);
        Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default);
        Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default);
    }
}
