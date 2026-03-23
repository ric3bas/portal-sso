using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Domain.Responses;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<Result<LoginResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
        Task<Result<bool>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
        Task<Result<RecuperarSenhaResponse>> SolicitarRecuperacaoAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken);
        Task<Result<ValidarTokenRecuperacaoResponse>> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken);
        Task<Result<TrocarSenhaResponse>> TrocarSenhaAsync(TrocarSenhaRequest request, CancellationToken cancellationToken);
     }
}
