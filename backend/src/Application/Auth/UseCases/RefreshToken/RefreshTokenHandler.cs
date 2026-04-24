using Portal.Domain.Base;
using AuthRepo = Portal.Domain.Usuario.Interfaces.IAuthRepository;
using TokenRepo = Portal.Domain.Usuario.Interfaces.ITokenAtualizacaoRepository;
using Portal.Domain.Usuario;
using Portal.Application.Auth.UseCases.Login;

namespace Portal.Application.Auth.UseCases.RefreshToken;

public class RefreshTokenHandler
{
    private readonly AuthRepo _authRepository;
    private readonly TokenRepo _tokenRepo;
    private readonly IConfiguration _config;

    public RefreshTokenHandler(AuthRepo authRepository, TokenRepo tokenRepo, IConfiguration config)
    {
        _authRepository = authRepository;
        _tokenRepo = tokenRepo;
        _config = config;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<LoginResponse>(request.ObterErros());

        var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);
        if (token is null || token.Revogado || token.ExpiraEm < DateTime.UtcNow)
            return Result.BusinessResult<LoginResponse>("Refresh token invÃ¡lido ou expirado");

        var dados = await _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId, cancellationToken);
        if (dados.Usuario is null)
            return Result.BusinessResult<LoginResponse>("UsuÃ¡rio nÃ£o encontrado");

        var jwtSection = _config.GetSection("Jwt");
        var accessTokenMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");
        var refreshTokenExpireDays = int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0");
        var novoRefreshToken = TokenBase.GenerateRefreshToken();

        var accessToken = JwtBase.GenerateToken(
            novoRefreshToken,
            dados.Usuario.Login,
            dados.Usuario.Nome,
            dados.Usuario.Email,
            accessTokenMinutes,
            dados.Usuario.ParceiroId.ToString(),
            jwtSection["Key"] ?? string.Empty,
            jwtSection["Issuer"] ?? string.Empty,
            jwtSection["Audience"] ?? string.Empty,
            dados.Perfil?.IsMaster,
            dados.Usuario.CorPrimaria,
            dados.Usuario.CorSecundaria,
            dados.Escopos.ToArray());

        await _tokenRepo.RevogarAsync(request.RefreshToken, cancellationToken);
        await _tokenRepo.InserirAsync(new TokenAtualizacaoCommand
        {
            Token = novoRefreshToken,
            ExpiraEm = DateTime.UtcNow.AddDays(refreshTokenExpireDays),
            Revogado = false,
            UsuarioId = token.UsuarioId,
            IpUsuario = token?.IpUsuario ?? string.Empty,
            LogadoEm = DateTime.UtcNow
        });

        return Result.OkResult(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = novoRefreshToken,
            ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
        });
    }
}
