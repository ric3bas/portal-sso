using Portal.Domain.Base;

namespace Portal.Features.Perfil.Domain.Interfaces
{
    public interface IPerfilService
    {
        Task<Result<IEnumerable<PerfilComEscopoResponse>>> ListarComEscoposAsync(CancellationToken cancellationToken = default);
        Task<Result<string>> CriarAsync(PerfilRequest request, CancellationToken cancellationToken = default);
        Task<Result<PerfilComEscopoResponse>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<string>> VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default);
        Task<Result<string>> ApagarAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<string>> ClonarAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<string>> AtualizarNomeAsync(int id, string novoNome, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<PerfilResponse>>> ObterPerfilParaComboAsync(CancellationToken cancellationToken = default);
    }
}
