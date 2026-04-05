using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Domain.Responses;
using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<Result<LoginResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
        Task<Result<bool>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
        Task<Result<string>> EsqueceSenhaAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken);
        Task<Result<string>> EsqueceSenhaLogadoAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken);
        Task<Result<string>> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken);
        Task<Result<string>> TrocarSenhaAsync(TrocarSenhaRequest request, CancellationToken cancellationToken);
        Task<TokenAtualizacaoQuery?> ObterTokenSessaoAsync(string token);
     }
}
