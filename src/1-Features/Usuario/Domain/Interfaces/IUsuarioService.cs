using Portal.Domain.Base;
using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioService
    {
        Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result<IEnumerable<UsuarioComPerfilQuery>>> ListarAsync(CancellationToken cancellationToken = default);
        Task IncrementarTentativaLogin(int UsuarioId, CancellationToken cancellationToken);
        Task ResetarTentativasLogin(int UsuarioId, CancellationToken cancellationToken);
    }
}
