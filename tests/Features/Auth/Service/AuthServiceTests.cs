using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Portal.Domain.Base.Email;
using Portal.Domain.Exceptions;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Infra;
using Portal.Features.Usuario.Service;

namespace sso.services;

public class AuthServiceTests
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenAtualizacaoRepository _tokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _authRepository = Substitute.For<IAuthRepository>();
        _tokenRepository = Substitute.For<ITokenAtualizacaoRepository>();
        _configuration = Substitute.For<IConfiguration>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _emailService = Substitute.For<IEmailService>();
        _usuarioRepository = Substitute.For<IUsuarioRepository>();

        _service = new AuthService(
            _authRepository,
            _tokenRepository,
            _configuration,
            _httpContextAccessor,
            _emailService,
            _usuarioRepository);
    }

    [Fact]
    public void Constructor_InitializesAllDependencies()
    {
        // Arrange
        var authRepository = Substitute.For<IAuthRepository>();
        var tokenRepository = Substitute.For<ITokenAtualizacaoRepository>();
        var configuration = Substitute.For<IConfiguration>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var emailService = Substitute.For<IEmailService>();
        var usuarioRepository = Substitute.For<IUsuarioRepository>();

        // Act
        var service = new AuthService(
            authRepository,
            tokenRepository,
            configuration,
            httpContextAccessor,
            emailService,
            usuarioRepository);

        // Assert
        Assert.NotNull(service);
    }

    #region TrocarSenhaAsync Tests

    [Fact]
    public async Task TrocarSenhaAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange - invalid request without token
        var request = new TrocarSenhaRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.TrocarSenhaAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task TrocarSenhaAsync_TokenNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new TrocarSenhaRequest
        {
            Token = "invalid-token",
            NovaSenha = "NewPassword123!",
            ConfirmarSenha = "NewPassword123!"
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns((RecuperacaoSenhaQuery?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.TrocarSenhaAsync(request, CancellationToken.None));
        Assert.Contains("Token inválido", exception.Errors);
    }

    [Fact]
    public async Task TrocarSenhaAsync_TokenAlreadyUsed_ThrowsBusinessException()
    {
        // Arrange
        var request = new TrocarSenhaRequest
        {
            Token = "used-token",
            NovaSenha = "NewPassword123!",
            ConfirmarSenha = "NewPassword123!"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "used-token",
            Usado = true,
            ExpiraEm = DateTime.UtcNow.AddHours(1),
            UsuarioId = 1
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.TrocarSenhaAsync(request, CancellationToken.None));
        Assert.Contains("Token já utilizado", exception.Errors);
    }

    [Fact]
    public async Task TrocarSenhaAsync_TokenExpired_ThrowsBusinessException()
    {
        // Arrange
        var request = new TrocarSenhaRequest
        {
            Token = "expired-token",
            NovaSenha = "NewPassword123!",
            ConfirmarSenha = "NewPassword123!"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "expired-token",
            Usado = false,
            ExpiraEm = DateTime.UtcNow.AddHours(-1),
            UsuarioId = 1
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.TrocarSenhaAsync(request, CancellationToken.None));
        Assert.Contains("Token expirado", exception.Errors);
    }

    [Fact]
    public async Task TrocarSenhaAsync_ValidRequest_UpdatesPasswordAndMarksTokenAsUsed()
    {
        // Arrange
        var request = new TrocarSenhaRequest
        {
            Token = "valid-token",
            NovaSenha = "NewPassword123!",
            ConfirmarSenha = "NewPassword123!"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "valid-token",
            Usado = false,
            ExpiraEm = DateTime.UtcNow.AddHours(1),
            UsuarioId = 123
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);
        _authRepository.AtualizarSenhaUsuarioAsync(entity.UsuarioId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.TrocarSenhaAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Senha alterada com sucesso", result.Mensagem);
        await _authRepository.Received(1).AtualizarSenhaUsuarioAsync(
            entity.UsuarioId,
            Arg.Is<string>(s => BCrypt.Net.BCrypt.Verify(request.NovaSenha, s)),
            Arg.Any<CancellationToken>());
        await _authRepository.Received(1).MarcarRecuperacaoSenhaComoUsadoAsync(entity.Id, Arg.Any<CancellationToken>());
    }

    #endregion

    #region ValidarTokenAsync Tests

    [Fact]
    public async Task ValidarTokenAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange - invalid request without token
        var request = new ValidarTokenRecuperacaoRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ValidarTokenAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ValidarTokenAsync_TokenNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new ValidarTokenRecuperacaoRequest
        {
            Token = "invalid-token"
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns((RecuperacaoSenhaQuery?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.ValidarTokenAsync(request, CancellationToken.None));
        Assert.Contains("Token inválido", exception.Errors);
    }

    [Fact]
    public async Task ValidarTokenAsync_TokenAlreadyUsed_ThrowsBusinessException()
    {
        // Arrange
        var request = new ValidarTokenRecuperacaoRequest
        {
            Token = "used-token"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "used-token",
            Usado = true,
            ExpiraEm = DateTime.UtcNow.AddHours(1),
            UsuarioId = 1
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.ValidarTokenAsync(request, CancellationToken.None));
        Assert.Contains("Token já utilizado", exception.Errors);
    }

    [Fact]
    public async Task ValidarTokenAsync_TokenExpired_ThrowsBusinessException()
    {
        // Arrange
        var request = new ValidarTokenRecuperacaoRequest
        {
            Token = "expired-token"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "expired-token",
            Usado = false,
            ExpiraEm = DateTime.UtcNow.AddHours(-1),
            UsuarioId = 1
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.ValidarTokenAsync(request, CancellationToken.None));
        Assert.Contains("Token expirado", exception.Errors);
    }

    [Fact]
    public async Task ValidarTokenAsync_ValidToken_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ValidarTokenRecuperacaoRequest
        {
            Token = "valid-token"
        };

        var entity = new RecuperacaoSenhaQuery
        {
            Id = 1,
            Token = "valid-token",
            Usado = false,
            ExpiraEm = DateTime.UtcNow.AddHours(1),
            UsuarioId = 1
        };

        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _service.ValidarTokenAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Token válido, pode prosseguir com alteração de senha", result.Mensagem);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange - invalid request without login/password
        var request = new LoginRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Login = "test@example.com",
            Senha = "password"
        };

        var dados = new LoginDataQuery
        {
            Usuario = null,
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(dados);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
        Assert.Contains("Usuário ou senha inválidos", exception.Errors);
    }

    [Fact]
    public async Task LoginAsync_UserBlockedByLoginAttempts_ThrowsBusinessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Login = "test@example.com",
            Senha = "password"
        };

        var dados = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("password"),
                TentativasLogin = 5,
                ParceiroId = Guid.NewGuid()
            },
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(dados);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
        Assert.Contains("Usuário bloqueado por excesso de tentativas. Aguarde ou redefina sua senha.", exception.Errors);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_IncrementsAttemptsAndThrowsBusinessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Login = "test@example.com",
            Senha = "wrongpassword"
        };

        var dados = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                TentativasLogin = 2,
                ParceiroId = Guid.NewGuid()
            },
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(dados);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
        Assert.Contains("Usuário ou senha inválidos", exception.Errors);
        await _usuarioRepository.Received(1).IncrementarTentativaLoginAsync(dados.Usuario.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ResetsAttemptsAndReturnsTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Login = "test@example.com",
            Senha = "correctpassword"
        };

        var parceiroId = Guid.NewGuid();
        var dados = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Nome = "Test User",
                Email = "test@example.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                TentativasLogin = 2,
                ParceiroId = parceiroId
            },
            Perfil = new UsuarioPerfilItemQuery { Nome = "Admin" },
            Escopos = new List<string> { "read", "write" }
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(dados);

        var configSection = Substitute.For<IConfigurationSection>();
        configSection["AccessTokenExpireMinutes"].Returns("60");
        configSection["RefreshTokenExpireDays"].Returns("7");
        configSection["Key"].Returns("test-secret-key-with-at-least-32-characters");
        configSection["Issuer"].Returns("test-issuer");
        configSection["Audience"].Returns("test-audience");
        _configuration.GetSection("Jwt").Returns(configSection);

        // Act
        var result = await _service.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("60", result.ExpireInMinutes);
        await _usuarioRepository.Received(1).ResetarTentativasLoginAsync(dados.Usuario.Id, Arg.Any<CancellationToken>());
        await _tokenRepository.Received(1).InserirAsync(Arg.Is<TokenAtualizacaoCommand>(t =>
            t.Token == result.RefreshToken &&
            t.Revogado == false &&
            t.UsuarioId == dados.Usuario.Id));
    }

    #endregion

    #region RefreshAsync Tests

    [Fact]
    public async Task RefreshAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange - invalid request without refresh token
        var request = new RefreshTokenRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.RefreshAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-token"
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns((TokenAtualizacaoQuery?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.RefreshAsync(request, CancellationToken.None));
        Assert.Contains("Refresh token inválido ou expirado", exception.Errors);
    }

    [Fact]
    public async Task RefreshAsync_TokenRevoked_ThrowsBusinessException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "revoked-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "revoked-token",
            Revogado = true,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.RefreshAsync(request, CancellationToken.None));
        Assert.Contains("Refresh token inválido ou expirado", exception.Errors);
    }

    [Fact]
    public async Task RefreshAsync_TokenExpired_ThrowsBusinessException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "expired-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "expired-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(-1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.RefreshAsync(request, CancellationToken.None));
        Assert.Contains("Refresh token inválido ou expirado", exception.Errors);
    }

    [Fact]
    public async Task RefreshAsync_UserNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        var dados = new LoginDataQuery
        {
            Usuario = null,
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId, Arg.Any<CancellationToken>())
            .Returns(dados);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.RefreshAsync(request, CancellationToken.None));
        Assert.Contains("Usuário não encontrado", exception.Errors);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RevokesOldTokenAndReturnsNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        var parceiroId = Guid.NewGuid();
        var dados = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Nome = "Test User",
                Email = "test@example.com",
                ParceiroId = parceiroId
            },
            Perfil = new UsuarioPerfilItemQuery { Nome = "Admin" },
            Escopos = new List<string> { "read", "write" }
        };

        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId, Arg.Any<CancellationToken>())
            .Returns(dados);

        var configSection = Substitute.For<IConfigurationSection>();
        configSection["AccessTokenExpireMinutes"].Returns("60");
        configSection["RefreshTokenExpireDays"].Returns("7");
        configSection["Key"].Returns("test-secret-key-with-at-least-32-characters");
        configSection["Issuer"].Returns("test-issuer");
        configSection["Audience"].Returns("test-audience");
        _configuration.GetSection("Jwt").Returns(configSection);

        // Act
        var result = await _service.RefreshAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("60", result.ExpireInMinutes);
        await _tokenRepository.Received(1).RevogarAsync(request.RefreshToken, Arg.Any<CancellationToken>());
        await _tokenRepository.Received(1).InserirAsync(Arg.Is<TokenAtualizacaoCommand>(t =>
            t.Token == result.RefreshToken &&
            t.Revogado == false &&
            t.UsuarioId == token.UsuarioId));
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange - invalid request without refresh token
        var request = new LogoutRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LogoutAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LogoutAsync_TokenNotFound_ThrowsBusinessException()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "invalid-token"
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns((TokenAtualizacaoQuery?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.LogoutAsync(request, CancellationToken.None));
        Assert.Contains("Refresh token inválido ou já revogado", exception.Errors);
    }

    [Fact]
    public async Task LogoutAsync_TokenAlreadyRevoked_ThrowsBusinessException()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "revoked-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "revoked-token",
            Revogado = true,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _service.LogoutAsync(request, CancellationToken.None));
        Assert.Contains("Refresh token inválido ou já revogado", exception.Errors);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "valid-token"
        };

        var token = new TokenAtualizacaoQuery
        {
            Id = 1,
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };

        _tokenRepository.ObterPorTokenAsync(request.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        await _service.LogoutAsync(request, CancellationToken.None);

        // Assert
        await _tokenRepository.Received(1).RevogarAsync(request.RefreshToken, Arg.Any<CancellationToken>());
    }

    #endregion

    #region SolicitarRecuperacaoAsync Tests

    [Fact]
    public async Task SolicitarRecuperacaoAsync_UserNotFound_ReturnsFalseEmailEnviado()
    {
        // Arrange
        var request = new RecuperarSenhaRequest
        {
            Login = "nonexistent@example.com"
        };

        var loginData = new LoginDataQuery
        {
            Usuario = null,
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(loginData);

        // Act
        var result = await _service.SolicitarRecuperacaoAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.EmailEnviado);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_UserWithoutEmail_ReturnsFalseEmailEnviado()
    {
        // Arrange
        var request = new RecuperarSenhaRequest
        {
            Login = "test@example.com"
        };

        var loginData = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Email = null
            },
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(loginData);

        // Act
        var result = await _service.SolicitarRecuperacaoAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.EmailEnviado);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_UserWithEmptyEmail_ReturnsFalseEmailEnviado()
    {
        // Arrange
        var request = new RecuperarSenhaRequest
        {
            Login = "test@example.com"
        };

        var loginData = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 1,
                Login = "test@example.com",
                Email = string.Empty
            },
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(loginData);

        // Act
        var result = await _service.SolicitarRecuperacaoAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.EmailEnviado);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_ValidUser_InsertsRecoveryTokenAndSendsEmail()
    {
        // Arrange
        var request = new RecuperarSenhaRequest
        {
            Login = "test@example.com"
        };

        var loginData = new LoginDataQuery
        {
            Usuario = new UsuarioQuery
            {
                Id = 123,
                Login = "test@example.com",
                Email = "test@example.com",
                Nome = "Test User"
            },
            Perfil = null,
            Escopos = new List<string>()
        };

        _authRepository.ObterDadosLoginAsync(request.Login, Arg.Any<CancellationToken>())
            .Returns(loginData);

        // Act
        var result = await _service.SolicitarRecuperacaoAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.EmailEnviado);
        
        await _authRepository.Received(1).InserirRecuperacaoSenhaAsync(
            Arg.Is<RecuperacaoSenhaCommand>(e =>
                e.UsuarioId == 123 &&
                !string.IsNullOrEmpty(e.Token) &&
                e.Usado == false &&
                e.ExpiraEm > DateTime.UtcNow));

        await _emailService.Received(1).EnviarEmailAsync(
            "test@example.com",
            "Recuperação de Senha",
            Arg.Is<string>(corpo => corpo.Contains("<html>") && corpo.Contains("Link")));
    }

    #endregion
}
