using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioService
    {
        Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<IEnumerable<UsuarioComPerfilQuery>> ListarAsync(CancellationToken cancellationToken = default);
        Task IncrementarTentativaLogin(int UsuarioId, CancellationToken cancellationToken);
        Task ResetarTentativasLogin(int UsuarioId, CancellationToken cancellationToken);
    }
}
