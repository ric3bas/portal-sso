using Portal.Dominio;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Infra;
using System.IdentityModel.Tokens.Jwt;
using static Portal.Features.Auth.Controller.AuthController;

namespace Portal.Features.Auth.Service
{

    public class AuthService : BaseService, IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        private readonly ITokenAtualizacaoRepository _tokenRepo;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Portal.Infra.Email.IEmailService _emailService;

        public AuthService(IAuthRepository authRepo, ITokenAtualizacaoRepository tokenRepo, IConfiguration config, IUnitOfWork unitOfWork,
             IHttpContextAccessor httpContextAccessor, Portal.Infra.Email.IEmailService emailService, IUsuarioRepository usuarioRepository) : base(httpContextAccessor)
        {
            _authRepository = authRepo;
            _config = config;
            _tokenRepo = tokenRepo;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<TrocarSenhaResponse> TrocarSenhaAsync(TrocarSenhaRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

            #region ValidaRetorno
            if (entity == null)
                throw new BusinessException("Token inválido");

            if (entity.Usado)
                throw new BusinessException("Token já utilizado");

            if (entity.ExpiraEm < DateTime.UtcNow)
                throw new BusinessException("Token expirado");
            #endregion

            var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

            _ = await _authRepository.AtualizarSenhaUsuarioAsync(entity.UsuarioId, novaSenhaHash, cancellationToken);

            await _authRepository.MarcarRecuperacaoSenhaComoUsadoAsync(entity.Id, cancellationToken);

            return new TrocarSenhaResponse { Mensagem = "Senha alterada com sucesso" };
        }

        public async Task<ValidarTokenRecuperacaoResponse> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

            #region ValidaRetorno
            if (entity == null)
                throw new BusinessException("Token inválido");

            if (entity.Usado)
                throw new BusinessException("Token já utilizado");

            if (entity.ExpiraEm < DateTime.UtcNow)
                throw new BusinessException("Token expirado");
            #endregion

            return new ValidarTokenRecuperacaoResponse { Mensagem = "Token válido, pode prosseguir com alteração de senha"};
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var dados = await _authRepository.ObterDadosLoginAsync(request.Login, cancellationToken);

            if (dados.Usuario is null)
                throw new BusinessException("Usuário ou senha inválidos");

            // Controle de tentativas de login
            if (dados.Usuario.TentativasLogin >= 5)
                throw new BusinessException("Usuário bloqueado por excesso de tentativas. Aguarde ou redefina sua senha.");

            if (!BCrypt.Net.BCrypt.Verify(request.Senha, dados.Usuario.Senha))
            {
                // Incrementa tentativas
                await _usuarioRepository.IncrementarTentativaLoginAsync(dados.Usuario.Id, cancellationToken);
                throw new BusinessException("Usuário ou senha inválidos");
            }

            // Login bem-sucedido: zera tentativas
            await _usuarioRepository.ResetarTentativasLoginAsync(dados.Usuario.Id, cancellationToken);

            var jwtSection = _config.GetSection("Jwt");
            var accessTokenExpireMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");

            var accessToken = JwtHelper.GenerateToken(
                dados.Usuario.Login   ?? string.Empty,
                dados.Usuario.Nome    ?? string.Empty,
                dados.Usuario.Email   ?? string.Empty,
                accessTokenExpireMinutes,
                dados.Usuario.ParceiroId.ToString(),
                jwtSection["Key"]      ?? string.Empty,
                jwtSection["Issuer"]   ?? string.Empty,
                jwtSection["Audience"] ?? string.Empty,
                dados.Perfil?.Nome ?? string.Empty,
                dados.Escopos.ToArray());

            var refreshToken = Token.GenerateRefreshToken();

            _unitOfWork.Begin();
            try
            {
                await _tokenRepo.InserirAsync(new TokenAtualizacao
                {
                    Token     = refreshToken,
                    ExpiraEm  = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0")),
                    Revogado  = false,
                    UsuarioId = dados.Usuario.Id
                });
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }

            return new LoginResponse
            {
                AccessToken     = accessToken,
                RefreshToken    = refreshToken,
                ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
            };
        }

        public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);

            if (token is null || token.Revogado || token.ExpiraEm < DateTime.UtcNow)
                throw new BusinessException("Refresh token inválido ou expirado");

            var dados = await _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId, cancellationToken);

            if (dados.Usuario is null)
                throw new BusinessException("Usuário não encontrado");

            var jwtSection             = _config.GetSection("Jwt");
            var accessTokenMinutes     = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "0");
            var refreshTokenExpireDays = int.Parse(jwtSection["RefreshTokenExpireDays"] ?? "0");

            var accessToken = JwtHelper.GenerateToken(
                dados.Usuario.Login   ?? string.Empty,
                dados.Usuario.Nome    ?? string.Empty,
                dados.Usuario.Email   ?? string.Empty,
                accessTokenMinutes,
                dados.Usuario.ParceiroId.ToString(),
                jwtSection["Key"]      ?? string.Empty,
                jwtSection["Issuer"]   ?? string.Empty,
                jwtSection["Audience"] ?? string.Empty,
                dados.Perfil?.Nome ?? string.Empty,
                dados.Escopos.ToArray());

            var novoRefreshToken = Token.GenerateRefreshToken();

            _unitOfWork.Begin();
            try
            {
                await _tokenRepo.RevogarAsync(request.RefreshToken, cancellationToken);
                await _tokenRepo.InserirAsync(new TokenAtualizacao
                {
                    Token     = novoRefreshToken,
                    ExpiraEm  = DateTime.UtcNow.AddDays(refreshTokenExpireDays),
                    Revogado  = false,
                    UsuarioId = token.UsuarioId
                });
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }

            return new LoginResponse
            {
                AccessToken     = accessToken,
                RefreshToken    = novoRefreshToken,
                ExpireInMinutes = jwtSection["AccessTokenExpireMinutes"] ?? "0"
            };
        }

        public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);

            if (token is null || token.Revogado)
                throw new BusinessException("Refresh token inválido ou já revogado");

            await _tokenRepo.RevogarAsync(request.RefreshToken, cancellationToken);
        }

        public async Task<RecuperarSenhaResponse> SolicitarRecuperacaoAsync(RecuperarSenhaRequest request, CancellationToken cancellationToken)
        {
            var loginData = await _authRepository.ObterDadosLoginAsync(request.Login);
            var usuario = loginData.Usuario;
            if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                return new RecuperarSenhaResponse { EmailEnviado = false };

            var token = Token.GerarToken();
            var entity = new RecuperacaoSenhaEntity
            {
                UsuarioId = usuario.Id,
                Token = token,
                ExpiraEm = DateTime.UtcNow.AddMinutes(15),
                Usado = false
            };
            await _authRepository.InserirRecuperacaoSenhaAsync(entity);

            var assunto = "Recuperação de Senha";
            var url = $"https://localhost:44349/api/v1/auth/validar-token?token={token}";
            var corpo = $@"<html>
                              <body>
                                <p>Olá,</p>
                                <p>Clique no <a href=""{url}"">Link</a> para alterar sua senha.</p>
                                <p>Se você não solicitou a alteração, ignore este e-mail.</p>
                              </body>
                            </html>";
            await _emailService.EnviarEmailAsync(usuario.Email, assunto, corpo);
            return new RecuperarSenhaResponse { EmailEnviado = true };
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
