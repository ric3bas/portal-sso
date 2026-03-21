using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
        Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
        Task<RecuperarSenhaResponse> SolicitarRecuperacaoAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken);
        Task<ValidarTokenRecuperacaoResponse> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken);
        Task<TrocarSenhaResponse> TrocarSenhaAsync(TrocarSenhaRequest request, CancellationToken cancellationToken);
     }
}
