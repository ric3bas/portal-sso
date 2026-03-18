using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Auth.Domain;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;
using Portal.Features.Auth.Service;
using Portal.Infra;
using Portal.Infra.Email;

namespace sso.services;

public class AuthServiceTests
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenAtualizacaoRepository _tokenRepo;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfigurationSection _jwtSection;
    private readonly IEmailService _emailService;
    public AuthServiceTests()
    {
        _authRepository = Substitute.For<IAuthRepository>();
        _tokenRepo = Substitute.For<ITokenAtualizacaoRepository>();
        _config = Substitute.For<IConfiguration>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _jwtSection = Substitute.For<IConfigurationSection>();
        _emailService = Substitute.For<IEmailService>();

        _config.GetSection("Jwt").Returns(_jwtSection);
        _jwtSection["AccessTokenExpireMinutes"].Returns("30");
        _jwtSection["RefreshTokenExpireDays"].Returns("7");
        _jwtSection["Key"].Returns("test-key-with-at-least-32-characters-for-security");
        _jwtSection["Issuer"].Returns("test-issuer");
        _jwtSection["Audience"].Returns("test-audience");
       
    }

    private AuthService CreateService()
    {
        return new AuthService(_authRepository, _tokenRepo, _config, _unitOfWork, _httpContextAccessor, _emailService);
    }

    [Fact]
    public void Constructor_InitializesAllFields()
    {
        var service = new AuthService(_authRepository, _tokenRepo, _config, _unitOfWork, _httpContextAccessor, _emailService);

        Assert.NotNull(service);
    }

    [Fact]
    public async Task LoginAsync_InvalidRequest_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new LoginRequest { Login = "", Senha = "" };

        await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new LoginRequest { Login = "testuser", Senha = "password" };
        var loginData = new LoginData { Usuario = null, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LoginAsync(request));

        Assert.Contains("Usuário ou senha inválidos", ex.Errors);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsBusinessException()
    {
        var service = CreateService();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var request = new LoginRequest { Login = "testuser", Senha = "wrongpassword" };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LoginAsync(request));

        Assert.Contains("Usuário ou senha inválidos", ex.Errors);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var perfil = new UsuarioPerfilItemResponse { Id = 1, Nome = "Admin" };
        var escopos = new List<string> { "read", "write" };
        var loginData = new LoginData { Usuario = usuario, Perfil = perfil, Escopos = escopos };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("30", result.ExpireInMinutes);
        await _tokenRepo.Received(1).InserirAsync(Arg.Any<TokenAtualizacao>());
        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Commit();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentialsWithNullPerfil_ReturnsLoginResponse()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var escopos = new List<string> { "read" };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = escopos };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_TokenInsertionFails_RollsBackAndThrows()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);
        _tokenRepo.InserirAsync(Arg.Any<TokenAtualizacao>()).Throws(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.Received(0).Commit();
    }

    [Fact]
    public async Task LoginAsync_UsesConfigurationValues()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        await service.LoginAsync(request);

        _config.Received(1).GetSection("Jwt");
        var _ = _jwtSection.Received()["AccessTokenExpireMinutes"];
        var __ = _jwtSection.Received()["RefreshTokenExpireDays"];
        var ___ = _jwtSection.Received()["Key"];
        var ____ = _jwtSection.Received()["Issuer"];
        var _____ = _jwtSection.Received()["Audience"];
    }

    [Fact]
    public async Task RefreshAsync_EmptyRefreshToken_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.RefreshAsync(request));

        Assert.Contains("Campo Refresh Token obrigatório", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_WhitespaceRefreshToken_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "   " };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.RefreshAsync(request));

        Assert.Contains("Campo Refresh Token obrigatório", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns((TokenAtualizacao?)null);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.RefreshAsync(request));

        Assert.Contains("Refresh token inválido ou expirado", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "revoked-token" };
        var token = new TokenAtualizacao
        {
            Token = "revoked-token",
            Revogado = true,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.RefreshAsync(request));

        Assert.Contains("Refresh token inválido ou expirado", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "expired-token" };
        var token = new TokenAtualizacao
        {
            Token = "expired-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(-1),
            UsuarioId = 1
        };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.RefreshAsync(request));

        Assert.Contains("Refresh token inválido ou expirado", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_UserNotFound_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        var loginData = new LoginData { Usuario = null, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.RefreshAsync(request));

        Assert.Contains("Usuário não encontrado", ex.Errors);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var perfil = new UsuarioPerfilItemResponse { Id = 1, Nome = "Admin" };
        var escopos = new List<string> { "read", "write" };
        var loginData = new LoginData { Usuario = usuario, Perfil = perfil, Escopos = escopos };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("30", result.ExpireInMinutes);
        await _tokenRepo.Received(1).RevogarAsync(request.RefreshToken);
        await _tokenRepo.Received(1).InserirAsync(Arg.Any<TokenAtualizacao>());
        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Commit();
    }

    [Fact]
    public async Task RefreshAsync_ValidTokenWithNullPerfil_ReturnsNewTokens()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var escopos = new List<string> { "read" };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = escopos };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task RefreshAsync_TokenOperationFails_RollsBackAndThrows()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);
        _tokenRepo.RevogarAsync(request.RefreshToken).Throws(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => service.RefreshAsync(request));

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.Received(0).Commit();
    }

    [Fact]
    public async Task RefreshAsync_UsesConfigurationValues()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        await service.RefreshAsync(request);

        _config.Received(1).GetSection("Jwt");
        var _ = _jwtSection.Received()["AccessTokenExpireMinutes"];
        var __ = _jwtSection.Received()["RefreshTokenExpireDays"];
        var ___ = _jwtSection.Received()["Key"];
        var ____ = _jwtSection.Received()["Issuer"];
        var _____ = _jwtSection.Received()["Audience"];
    }

    [Fact]
    public async Task LogoutAsync_EmptyRefreshToken_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new LogoutRequest { RefreshToken = "" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.LogoutAsync(request));

        Assert.Contains("Campo Refresh Token obrigatório", ex.Errors);
    }

    [Fact]
    public async Task LogoutAsync_WhitespaceRefreshToken_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new LogoutRequest { RefreshToken = "   " };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.LogoutAsync(request));

        Assert.Contains("Campo Refresh Token obrigatório", ex.Errors);
    }

    [Fact]
    public async Task LogoutAsync_TokenNotFound_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new LogoutRequest { RefreshToken = "invalid-token" };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns((TokenAtualizacao?)null);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LogoutAsync(request));

        Assert.Contains("Refresh token inválido ou já revogado", ex.Errors);
    }

    [Fact]
    public async Task LogoutAsync_AlreadyRevokedToken_ThrowsBusinessException()
    {
        var service = CreateService();
        var request = new LogoutRequest { RefreshToken = "revoked-token" };
        var token = new TokenAtualizacao
        {
            Token = "revoked-token",
            Revogado = true,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LogoutAsync(request));

        Assert.Contains("Refresh token inválido ou já revogado", ex.Errors);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        var service = CreateService();
        var request = new LogoutRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);

        await service.LogoutAsync(request);

        await _tokenRepo.Received(1).RevogarAsync(request.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithNullUsuarioFields_UsesEmptyStringFallbacks()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = null,
            Senha = hashedPassword,
            Nome = null,
            Email = null,
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithNullJwtConfigValues_UsesEmptyStringFallbacks()
    {
        var service = CreateService();
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Login = "testuser", Senha = password };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "testuser",
            Senha = hashedPassword,
            Nome = "Test User",
            Email = "test@example.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _authRepository.ObterDadosLoginAsync(request.Login).Returns(loginData);

        _jwtSection["Issuer"].Returns((string?)null);
        _jwtSection["Audience"].Returns((string?)null);
        _jwtSection["AccessTokenExpireMinutes"].Returns("30");
        _jwtSection["RefreshTokenExpireDays"].Returns("7");

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_WithNullUsuarioFields_UsesEmptyStringFallbacks()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = null,
            Nome = null,
            Email = null,
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_WithNullJwtConfigValues_UsesEmptyStringFallbacks()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "user",
            Nome = "User",
            Email = "u@u.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);

        _jwtSection["Issuer"].Returns((string?)null);
        _jwtSection["Audience"].Returns((string?)null);
        _jwtSection["AccessTokenExpireMinutes"].Returns("30");
        _jwtSection["RefreshTokenExpireDays"].Returns("7");

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task RefreshAsync_WithNullAccessTokenExpireMinutes_UsesZeroFallback()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "user",
            Nome = "User",
            Email = "u@u.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);
        _jwtSection["AccessTokenExpireMinutes"].Returns((string?)null);

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.Equal("0", result.ExpireInMinutes);
    }

    [Fact]
    public async Task RefreshAsync_WithNullRefreshTokenExpireDays_UsesZeroFallback()
    {
        var service = CreateService();
        var request = new RefreshTokenRequest { RefreshToken = "valid-token" };
        var token = new TokenAtualizacao
        {
            Token = "valid-token",
            Revogado = false,
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            UsuarioId = 1
        };
        var usuario = new UsuarioEntity
        {
            Id = 1,
            Login = "user",
            Nome = "User",
            Email = "u@u.com",
            ParceiroId = Guid.NewGuid()
        };
        var loginData = new LoginData { Usuario = usuario, Perfil = null, Escopos = new List<string>() };
        _tokenRepo.ObterPorTokenAsync(request.RefreshToken).Returns(token);
        _authRepository.ObterDadosLoginPorIdAsync(token.UsuarioId).Returns(loginData);
        _jwtSection["RefreshTokenExpireDays"].Returns((string?)null);

        var result = await service.RefreshAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.RefreshToken);
    }

    //[Fact]
    //public async Task LoginAsync_UsuarioRevogado_ThrowsBusinessException()
    //{
    //    var service = CreateService();
    //    var password = "password123";
    //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    //    var usuario = new UsuarioEntity
    //    {
    //        Id = 1,
    //        Login = "testuser",
    //        Senha = hashedPassword,
    //        Nome = "Test User",
    //        Email = "test@example.com",
    //        ParceiroId = Guid.NewGuid(),
    //        // Adicione propriedade de revogado/inativo se existir
    //    };
    //    var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
    //    _authRepository.ObterDadosLoginAsync("testuser").Returns(loginData);

    //    // Simule a condição de revogado/inativo conforme sua entidade
    //    // usuario.Ativo = false; // Exemplo

    //    var request = new LoginRequest { Login = "testuser", Senha = password };
    //    var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LoginAsync(request));
    //    Assert.Contains("Usuário inativo ou revogado", ex.Errors);
    //}

    //[Fact]
    //public async Task LoginAsync_SemEscopos_ThrowsBusinessException()
    //{
    //    var service = CreateService();
    //    var password = "password123";
    //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    //    var usuario = new UsuarioEntity
    //    {
    //        Id = 1,
    //        Login = "testuser",
    //        Senha = hashedPassword,
    //        Nome = "Test User",
    //        Email = "test@example.com",
    //        ParceiroId = Guid.NewGuid()
    //    };
    //    var loginData = new LoginData { Usuario = usuario, Escopos = new List<string>() };
    //    _authRepository.ObterDadosLoginAsync("testuser").Returns(loginData);

    //    var request = new LoginRequest { Login = "testuser", Senha = password };
    //    var ex = await Assert.ThrowsAsync<BusinessException>(() => service.LoginAsync(request));
    //    Assert.Contains("Usuário sem escopos", ex.Errors);
    //}

    [Fact]
    public async Task SolicitarRecuperacaoAsync_UsuarioNaoEncontrado_RetornaEmailNaoEnviado()
    {
        var service = CreateService();
        _authRepository.ObterDadosLoginAsync(Arg.Any<string>()).Returns(new LoginData { Usuario = null, Escopos = new List<string>() });

        var result = await service.SolicitarRecuperacaoAsync(new RecuperarSenhaRequest { Login = "missing" });

        Assert.False(result.EmailEnviado);
        await _authRepository.DidNotReceive().InserirRecuperacaoSenhaAsync(Arg.Any<RecuperacaoSenhaEntity>());
        await _emailService.DidNotReceive().EnviarEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_UsuarioEncontrado_EnviaEmail()
    {
        var service = CreateService();
        var usuario = new UsuarioEntity { Id = 99, Email = "user@teste.com", ParceiroId = Guid.NewGuid() };
        _authRepository.ObterDadosLoginAsync("user").Returns(new LoginData { Usuario = usuario, Escopos = new List<string>() });

        var result = await service.SolicitarRecuperacaoAsync(new RecuperarSenhaRequest { Login = "user" });

        Assert.True(result.EmailEnviado);
        await _authRepository.Received(1).InserirRecuperacaoSenhaAsync(Arg.Is<RecuperacaoSenhaEntity>(x => x.UsuarioId == usuario.Id));
        await _emailService.Received(1).EnviarEmailAsync(usuario.Email, Arg.Any<string>(), Arg.Is<string>(x => x.Contains("Clique no <a href")));
    }

    [Fact]
    public async Task ValidarTokenAsync_TokenValido_RetornaMensagem()
    {
        var service = CreateService();
        var entidade = new RecuperacaoSenhaEntity
        {
            UsuarioId = 1,
            Token = "token",
            ExpiraEm = DateTime.UtcNow.AddMinutes(5),
            Usado = false
        };
        _authRepository.ObterRecuperacaoSenhaPorTokenAsync("token").Returns(entidade);

        var result = await service.ValidarTokenAsync(new ValidarTokenRecuperacaoRequest { Token = "token" });

        Assert.Equal("Token válido, pode prosseguir com alteração de senha", result.Mensagem);
    }

    [Fact]
    public async Task ValidarTokenAsync_TokenInvalido_LancaBusinessException()
    {
        var service = CreateService();
        _authRepository.ObterRecuperacaoSenhaPorTokenAsync("token").Returns((RecuperacaoSenhaEntity?)null);

        await Assert.ThrowsAsync<BusinessException>(() => service.ValidarTokenAsync(new ValidarTokenRecuperacaoRequest { Token = "token" }));
    }

    [Fact]
    public async Task TrocarSenhaAsync_TokenValido_AtualizaSenha()
    {
        var service = CreateService();
        var entidade = new RecuperacaoSenhaEntity
        {
            Id = 10,
            UsuarioId = 5,
            Token = "token",
            ExpiraEm = DateTime.UtcNow.AddMinutes(5),
            Usado = false
        };
        _authRepository.ObterRecuperacaoSenhaPorTokenAsync(entidade.Token).Returns(entidade);
        _authRepository.AtualizarSenhaUsuarioAsync(entidade.UsuarioId, Arg.Any<string>()).Returns(true);

        var result = await service.TrocarSenhaAsync(new TrocarSenhaRequest { Token = "token", NovaSenha = "Nova@123", ConfirmarSenha = "Nova@123" });

        Assert.Equal("Senha alterada com sucesso", result.Mensagem);
        await _authRepository.Received(1).AtualizarSenhaUsuarioAsync(entidade.UsuarioId, Arg.Any<string>());
        await _authRepository.Received(1).MarcarRecuperacaoSenhaComoUsadoAsync(entidade.Id);
    }

    [Fact]
    public async Task TrocarSenhaAsync_TokenInvalido_LancaBusinessException()
    {
        var service = CreateService();
        _authRepository.ObterRecuperacaoSenhaPorTokenAsync("token").Returns((RecuperacaoSenhaEntity?)null);

        await Assert.ThrowsAsync<BusinessException>(() => service.TrocarSenhaAsync(new TrocarSenhaRequest { Token = "token", NovaSenha = "nova", ConfirmarSenha = "nova" }));
    }
}
