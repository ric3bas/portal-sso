using Portal._1_Features.Usuario.Domain.Responses;
using Portal.Domain.Base;
using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioService
    {
        Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result<IEnumerable<UsuarioComPerfilResponse>>> ListarPorParceiroAsync(string? parceiroId, CancellationToken cancellationToken = default);
        Task IncrementarTentativaLogin(int UsuarioId, CancellationToken cancellationToken);
        Task ResetarTentativasLogin(int UsuarioId, CancellationToken cancellationToken);
    }
}
