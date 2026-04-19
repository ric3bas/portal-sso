using Portal.Domain.Base;
using AuthRepo = Portal.Domain.Usuario.Interfaces.IAuthRepository;
using UserRepo = Portal.Domain.Usuario.Interfaces.IUsuarioRepository;
using TokenRepo = Portal.Domain.Usuario.Interfaces.ITokenAtualizacaoRepository;
using Portal.Domain.Usuario;

namespace Portal.Application.Auth.UseCases.Login;

public class LoginHandler
{
    private readonly AuthRepo _authRepository;
    private readonly UserRepo _usuarioRepository;
    private readonly TokenRepo _tokenRepo;
    private readonly IConfiguration _config;

    public LoginHandler(AuthRepo authRepository, UserRepo usuarioRepository, TokenRepo tokenRepo, IConfiguration config)
    {
        _authRepository = authRepository;
        _usuarioRepository = usuarioRepository;
        _tokenRepo = tokenRepo;
        _config = config;
    }

    public async Task<Result<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<LoginResponse>(request.ObterErros());

        var dados = await _authRepository.ObterDadosLoginAsync(request.Login, cancellationToken);

        if (dados?.Usuario is null)
            return Result.BusinessResult<LoginResponse>("Usuário ou senha inválidos");

        if (!dados.Usuario.Ativo)
            return Result.BusinessResult<LoginResponse>("Usuário inativo no sistema");

        var tentativaAtual = dados.Usuario.TentativasLogin + 1;

        if (dados.Usuario.Bloqueado)
            return Result.BusinessResult<LoginResponse>("Usuário bloqueado por excesso de tentativas");

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, dados.Usuario.Senha))
        {
            await _usuarioRepository.IncrementarTentativaLoginAsync(dados.Usuario.Id, cancellationToken);

            if (tentativaAtual == 5)
            {
                await _usuarioRepository.BloquearUsuarioAsync(dados.Usuario.Id, cancellationToken);
                return Result.BusinessResult<LoginResponse>("Usuário bloqueado por excesso de tentativas");
            }

            return Result.BusinessResult<LoginResponse>($"Usuário ou senha inválidos, voce tem mais {5 - tentativaAtual} tentativas");
        }

        await _usuarioRepository.ResetarTentativasLoginAsync(dados.Usuario.Id, cancellationToken);

        var jwtSection = _config.GetSection("Jwt");
        var accessTokenExpireMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");
        var refreshToken = TokenBase.GenerateRefreshToken();

        var accessToken = JwtBase.GenerateToken(
            refreshToken,
            dados.Usuario.Login,
            dados.Usuario.Nome,
            dados.Usuario.Email,
            accessTokenExpireMinutes,
            dados.Usuario.ParceiroId.ToString(),
            jwtSection["Key"] ?? string.Empty,
            jwtSection["Issuer"] ?? string.Empty,
            jwtSection["Audience"] ?? string.Empty,
            dados.Perfil?.IsMaster,
            dados.Escopos.ToArray());

        await _tokenRepo.InserirAsync(new TokenAtualizacaoCommand
        {
            Token = refreshToken,
            ExpiraEm = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0")),
            Revogado = false,
            UsuarioId = dados.Usuario.Id,
            IpUsuario = request.IpUsuario,
            LogadoEm = DateTime.UtcNow
        });

        return Result.OkResult(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
        });
    }
}
