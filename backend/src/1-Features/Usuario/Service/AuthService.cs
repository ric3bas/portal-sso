using Portal.Domain.Base;
using Portal.Domain.Base.Email;
using Portal.Domain.Exceptions;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Domain.Responses;
using Portal.Features.Usuario.Infra;
using Portal.Infra;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Usuario.Service
{

    public class AuthService : BaseService, IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenAtualizacaoRepository _tokenRepo;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository authRepo, ITokenAtualizacaoRepository tokenRepo, IConfiguration config,
             IHttpContextAccessor httpContextAccessor, IEmailService emailService, IUsuarioRepository usuarioRepository, IUnitOfWork unitOfWork) : base(httpContextAccessor)
        {
            _authRepository = authRepo;
            _config = config;
            _tokenRepo = tokenRepo;
            _emailService = emailService;
            _usuarioRepository = usuarioRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> TrocarSenhaAsync(TrocarSenhaRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                return ValidationResult<string>(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

            if (entity == null)
                return BusinessResult<string>("Token inválido");

            if (entity.Usado)
                return BusinessResult<string>("Token já utilizado");

            if (entity.ExpiraEm < DateTime.UtcNow)
                return BusinessResult<string>("Token expirado");

            var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

            _ = await _authRepository.AtualizarSenhaUsuarioAsync(entity.UsuarioId, novaSenhaHash, cancellationToken);

            await _authRepository.MarcarRecuperacaoSenhaComoUsadoAsync(entity.Id, cancellationToken);

            return OkResult("Senha alterada com sucesso");
        }

        public async Task<Result<string>> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                return ValidationResult<string>(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

            if (entity == null)
                return BusinessResult<string>("Token inválido");

            if (entity.Usado)
                return BusinessResult<string>("Token já utilizado");

            if (entity.ExpiraEm < DateTime.UtcNow)
                return BusinessResult<string>("Token expirado");

            return OkResult("Token válido, pode prosseguir com alteração de senha");
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                return ValidationResult<LoginResponse>(request.ObterErros());

            var dados = await _authRepository.ObterDadosLoginAsync(request.Login, cancellationToken);

            if (dados?.Usuario is null)
                return BusinessResult<LoginResponse>("Usuário ou senha inválidos");

            if (!dados.Usuario.Ativo)
                return BusinessResult<LoginResponse>("Usuário inativo no sistema");

            var tentativaAtual = dados.Usuario.TentativasLogin + 1;

            // Controle de tentativas de login
            if (dados.Usuario.Bloqueado)
                return BusinessResult<LoginResponse>("Usuário bloqueado por excesso de tentativas");

            if (!BCrypt.Net.BCrypt.Verify(request.Senha, dados.Usuario.Senha))
            {
                await _usuarioRepository.IncrementarTentativaLoginAsync(dados.Usuario.Id, cancellationToken);

                if (tentativaAtual == 5)
                {
                    await _usuarioRepository.BloquearUsuarioAsync(dados.Usuario.Id, cancellationToken);
                    return BusinessResult<LoginResponse>("Usuário bloqueado por excesso de tentativas");
                }

                return BusinessResult<LoginResponse>($"Usuário ou senha inválidos, voce tem mais {5 - tentativaAtual} tentativas");
            }

            // Login bem-sucedido: zera tentativas
            await _usuarioRepository.ResetarTentativasLoginAsync(dados.Usuario.Id, cancellationToken);

            var jwtSection = _config.GetSection("Jwt");
            var accessTokenExpireMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");
            var refreshToken = TokenBase.GenerateRefreshToken();

            var accessToken = JwtBase.GenerateToken(
                refreshToken,
                dados.Usuario.Login   ?? string.Empty,
                dados.Usuario.Nome    ?? string.Empty,
                dados.Usuario.Email   ?? string.Empty,
                accessTokenExpireMinutes,
                dados.Usuario.ParceiroId.ToString(),
                jwtSection["Key"]      ?? string.Empty,
                jwtSection["Issuer"]   ?? string.Empty,
                jwtSection["Audience"] ?? string.Empty,
                dados.Perfil?.isMaster,
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

            return OkResult(new LoginResponse
            {
                AccessToken     = accessToken,
                RefreshToken    = refreshToken,
                ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
            });
        }

        public async Task<Result<LoginResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                return ValidationResult<LoginResponse>(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);

            if (token is null || token.Revogado || token.ExpiraEm < DateTime.UtcNow)
                return BusinessResult<LoginResponse>("Refresh token inválido ou expirado");

            var dados = await _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId, cancellationToken);

            if (dados.Usuario is null)
                return BusinessResult<LoginResponse>("Usuário não encontrado");

            var jwtSection             = _config.GetSection("Jwt");
            var accessTokenMinutes     = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");
            var refreshTokenExpireDays = int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0");
            var novoRefreshToken = TokenBase.GenerateRefreshToken();


            var accessToken = JwtBase.GenerateToken(
                novoRefreshToken,
                dados.Usuario.Login   ?? string.Empty,
                dados.Usuario.Nome    ?? string.Empty,
                dados.Usuario.Email   ?? string.Empty,
                accessTokenMinutes,
                dados.Usuario.ParceiroId.ToString(),
                jwtSection["Key"]      ?? string.Empty,
                jwtSection["Issuer"]   ?? string.Empty,
                jwtSection["Audience"] ?? string.Empty,
                dados.Perfil?.isMaster,
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

            return OkResult(new LoginResponse
            {
                AccessToken     = accessToken,
                RefreshToken    = novoRefreshToken,
                ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
            });
        }

        public async Task<Result<bool>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                return ValidationResult<bool>(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);

            if (token is null || token.Revogado)
                return BusinessResult<bool>("Refresh token inválido ou já revogado");

            await _tokenRepo.RevogarAsync(request.RefreshToken, cancellationToken);
            return OkResult(true);
        }

        public async Task<Result<string>> EsqueceSenhaAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken)
        {
            var loginData = await _authRepository.ObterDadosLoginAsync(request.Login);
            var usuario = loginData.Usuario;
            if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                return BusinessResult<string>("Usuário não encontrado");

            if (!usuario.Ativo)
                return BusinessResult<string>("Usuário inativo no sistema");

            var token = TokenBase.GerarToken();
            var entity = new RecuperacaoSenhaCommand
            {
                UsuarioId = usuario.Id,
                Token = token,
                ExpiraEm = DateTime.UtcNow.AddMinutes(15),
                Usado = false
            };
            await _authRepository.InserirRecuperacaoSenhaAsync(entity);

            var assunto = "Recuperação de Senha";
            var url = $"https://localhost:44349/api/v1/auth/validar-token?token={token}";
            var corpo = $@"<!DOCTYPE html>
                            <html>
                              <body style='font-family: Arial, sans-serif;'>
                                <p>Olá,</p>
                                <p>Clique no 
                                  <a href='http://localhost:5173/auth/trocar-senha?token={token}'>
                                    Link
                                  </a> 
                                  para alterar sua senha.
                                </p>
                                <p>Se você não solicitou a alteração, ignore este e-mail.</p>
                              </body>
                            </html>";
            await _emailService.EnviarEmailAsync(usuario.Email, assunto, corpo);
            return OkResult("E-mail enviado com sucesso");
        }

        public async Task<Result<string>> EsqueceSenhaLogadoAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken)
        {
            var loginData = await _authRepository.ObterDadosLoginAsync(request.Login);
            var usuario = loginData.Usuario;
            if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                return BusinessResult<string>("Usuário não encontrado");

            if (!usuario.Ativo)
                return BusinessResult<string>("Usuário inativo no sistema");

            var token = TokenBase.GerarToken();
            var entity = new RecuperacaoSenhaCommand
            {
                UsuarioId = usuario.Id,
                Token = token,
                ExpiraEm = DateTime.UtcNow.AddMinutes(15),
                Usado = false
            };
            await _authRepository.InserirRecuperacaoSenhaAsync(entity);

            return OkResult(token);
        }


        public async Task<TokenAtualizacaoQuery?> ObterTokenSessaoAsync(string token)
        {
            return await _tokenRepo.ObterPorTokenAsync(token);
        }


        #region Google
        //public async Task<LoginResponse> LoginGoogle(string code)
        //{
        //    var client = new HttpClient();
        //    var jwtGoogleSection = _config.GetSection("Authentication.Google");

        //    var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token",
        //        new FormUrlEncodedContent(new Dictionary<string, string>
        //        {
        //            { "code", code },
        //            { "client_id", jwtGoogleSection["ClientId"] ?? string.Empty  },
        //            { "client_secret", jwtGoogleSection["ClientSecret"] ?? string.Empty  },
        //            { "redirect_uri", "https://localhost:44349/api/v1/auth/callback" },
        //            { "grant_type", "authorization_code" }
        //        }));

        //    var json = await tokenResponse.Content.ReadAsStringAsync();
        //    var tokens = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenResponse>(json);

        //    var handler = new JwtSecurityTokenHandler();
        //    var jwt = handler.ReadJwtToken(tokens?.Id_Token);

        //    var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        //    var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;


        //    var jwtSection = _config.GetSection("Jwt");
        //    var accessTokenExpireMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");

        //    var accessToken = JwtHelper.GenerateToken(
        //        email ?? string.Empty,
        //        name ?? string.Empty,
        //        email ?? string.Empty,
        //        accessTokenExpireMinutes,
        //        string.Empty,
        //        jwtSection["Key"] ?? string.Empty,
        //        jwtSection["Issuer"] ?? string.Empty,
        //        jwtSection["Audience"] ?? string.Empty,
        //        string.Empty,
        //        []);

        //    var refreshToken = Token.GenerateRefreshToken();

        //    _unitOfWork.Begin();
        //    try
        //    {
        //        await _tokenRepo.InserirAsync(new TokenAtualizacao
        //        {
        //            Token = refreshToken,
        //            ExpiraEm = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0")),
        //            Revogado = false,
        //            UsuarioId = default(int)
        //        });
        //        _unitOfWork.Commit();
        //    }
        //    catch
        //    {
        //        _unitOfWork.Rollback();
        //        throw;
        //    }

        //    return new LoginResponse
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken,
        //        ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
        //    };
        //}

        #endregion
    }
}
