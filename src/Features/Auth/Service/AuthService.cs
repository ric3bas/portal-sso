using Portal.Dominio;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;
using Portal.Infra;

namespace Portal.Features.Auth.Service
{

    public class AuthService : BaseService, IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenAtualizacaoRepository _tokenRepo;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Portal.Infra.Email.IEmailService _emailService;

        public AuthService(IAuthRepository authRepo, ITokenAtualizacaoRepository tokenRepo, IConfiguration config, IUnitOfWork unitOfWork,
             IHttpContextAccessor httpContextAccessor, Portal.Infra.Email.IEmailService emailService) : base(httpContextAccessor)
        {
            _authRepository = authRepo;
            _config         = config;
            _tokenRepo      = tokenRepo;
            _unitOfWork     = unitOfWork;
            _emailService   = emailService;
        }

        public async Task<TrocarSenhaResponse> TrocarSenhaAsync(TrocarSenhaRequest request)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token);

            #region ValidaRetorno
            if (entity == null)
                throw new BusinessException("Token inválido");

            if (entity.Usado)
                throw new BusinessException("Token já utilizado");

            if (entity.ExpiraEm < DateTime.UtcNow)
                throw new BusinessException("Token expirado");
            #endregion

            var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

            _ = await _authRepository.AtualizarSenhaUsuarioAsync(entity.UsuarioId, novaSenhaHash);

            await _authRepository.MarcarRecuperacaoSenhaComoUsadoAsync(entity.Id);

            return new TrocarSenhaResponse { Mensagem = "Senha alterada com sucesso" };
        }

        public async Task<ValidarTokenRecuperacaoResponse> ValidarTokenAsync(ValidarTokenRecuperacaoRequest request)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token);

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

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var dados = await _authRepository.ObterDadosLoginAsync(request.Login);

            if (dados.Usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, dados.Usuario.Senha))
                throw new BusinessException("Usuário ou senha inválidos");

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

        public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken);

            if (token is null || token.Revogado || token.ExpiraEm < DateTime.UtcNow)
                throw new BusinessException("Refresh token inválido ou expirado");

            var dados = await _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId);

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
                await _tokenRepo.RevogarAsync(request.RefreshToken);
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

        public async Task LogoutAsync(LogoutRequest request)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken);

            if (token is null || token.Revogado)
                throw new BusinessException("Refresh token inválido ou já revogado");

            await _tokenRepo.RevogarAsync(request.RefreshToken);
        }

        public async Task<RecuperarSenhaResponse> SolicitarRecuperacaoAsync(RecuperarSenhaRequest request)
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
    }
}
