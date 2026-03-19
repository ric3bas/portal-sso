using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshAsync(RefreshTokenRequest request);
        Task LogoutAsync(LogoutRequest request);
        Task<RecuperarSenhaResponse> SolicitarRecuperacaoAsync(RecuperarSenhaRequest request);
        Task<ValidarTokenRecuperacaoResponse> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request);
        Task<TrocarSenhaResponse> TrocarSenhaAsync(TrocarSenhaRequest request);
     }
}
