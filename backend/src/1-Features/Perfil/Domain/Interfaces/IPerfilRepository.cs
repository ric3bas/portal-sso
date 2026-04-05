using Portal.Features.Perfil.Infra;

namespace Portal.Features.Perfil.Domain.Interfaces
{
    public interface IPerfilRepository
    {
        Task<IEnumerable<PerfilComEscopoQuery>> ListarComEscoposAsync(CancellationToken cancellationToken = default);
        Task<int> InserirAsync(PerfilCommand perfil, CancellationToken cancellationToken = default);
        Task<PerfilComEscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistePerfilAsync(int perfilId, CancellationToken cancellationToken = default);
        Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default);
        Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default);
        Task DeletarAsync(int id, CancellationToken cancellationToken = default);
        Task AtualizarNomeAsync(int id, string novoNome, CancellationToken cancellationToken = default);
    }
}
