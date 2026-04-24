using Microsoft.AspNetCore.Http;
using NSubstitute;
using Portal.Domain.Exceptions;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Infra;
using Portal.Features.Usuario.Service;
using Portal.Infra;
using System.Security.Claims;

namespace sso.repositories;


public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPerfilRepository _perfilRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UsuarioService _service;
    private readonly Guid _testTenantId = Guid.NewGuid();

    public UsuarioServiceTests()
    {
        _usuarioRepository = Substitute.For<IUsuarioRepository>();
        _perfilRepository = Substitute.For<IPerfilRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        SetupHttpContext();

        _service = new UsuarioService(
            _usuarioRepository,
            _httpContextAccessor);
    }

    private void SetupHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim("tenantId", _testTenantId.ToString()) });
        httpContext.User = new ClaimsPrincipal(identity);
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void Constructor_InitializesAllDependencies()
    {
        var usuarioRepository = Substitute.For<IUsuarioRepository>();
        var perfilRepository = Substitute.For<IPerfilRepository>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var service = new UsuarioService(
            usuarioRepository,
            httpContextAccessor);

        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListarAsync_WithResults_ReturnsResults()
    {
        var usuarios = new List<UsuarioComPerfilQuery>
        {
            new UsuarioComPerfilQuery { Id = 1, Nome = "Usuario 1" },
            new UsuarioComPerfilQuery { Id = 2, Nome = "Usuario 2" }
        };
        _usuarioRepository.ListarAsync(_testTenantId, Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = await _service.ListarAsync(CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task ListarAsync_NoResults_ThrowsNotFoundException()
    {
        var usuarios = new List<UsuarioComPerfilQuery>();
        _usuarioRepository.ListarAsync(_testTenantId, Arg.Any<CancellationToken>()).Returns(usuarios);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ListarAsync(CancellationToken.None));
        Assert.Equal("Nenhum usuário encontrado", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_InsertsUsuario()
    {
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@test.com",
            Login = "testuser",
            Senha = "Password123",
            PerfilId = 1
        };

        var validacao = new RegistroValidacaoQuery
        {
            ParceiroExiste = true,
            LoginExiste = false
        };

        _usuarioRepository.ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            Arg.Any<CancellationToken>()).Returns(validacao);

        _usuarioRepository.InserirAsync(
            Arg.Any<UsuarioCommand>(),
            Arg.Any<CancellationToken>()).Returns(1);

        await _service.RegisterAsync(request, CancellationToken.None);

        await _usuarioRepository.Received(1).InserirAsync(
            Arg.Is<UsuarioCommand>(u =>
                u.Nome == request.Nome &&
                u.Email == request.Email &&
                u.Login == request.Login &&
                u.ParceiroId == _testTenantId &&
                u.PerfilId == request.PerfilId &&
                u.TentativasLogin == 0 &&
                u.UltimoErroLogin == null &&
                u.Bloqueado == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new RegisterRequest
        {
            Nome = "",
            Email = "",
            Login = "",
            Senha = "",
            PerfilId = 0
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.RegisterAsync(request, CancellationToken.None));
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_ParceiroNotFound_ThrowsNotFoundException()
    {
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@test.com",
            Login = "testuser",
            Senha = "Password123",
            PerfilId = 1
        };

        var validacao = new RegistroValidacaoQuery
        {
            ParceiroExiste = false,
            LoginExiste = false
        };

        _usuarioRepository.ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            Arg.Any<CancellationToken>()).Returns(validacao);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.RegisterAsync(request, CancellationToken.None));
        Assert.Equal($"Parceiro '{_testTenantId}' não encontrado", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_LoginExists_ThrowsValidationException()
    {
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@test.com",
            Login = "testuser",
            Senha = "Password123",
            PerfilId = 1
        };

        var validacao = new RegistroValidacaoQuery
        {
            ParceiroExiste = true,
            LoginExiste = true
        };

        _usuarioRepository.ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            Arg.Any<CancellationToken>()).Returns(validacao);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.RegisterAsync(request, CancellationToken.None));
        Assert.Contains($"Login '{request.Login}' já existe", exception.Errors);
    }

    [Fact]
    public async Task IncrementarTentativaLogin_CallsRepository()
    {
        var usuarioId = 1;

        await _service.IncrementarTentativaLogin(usuarioId, CancellationToken.None);

        await _usuarioRepository.Received(1).IncrementarTentativaLoginAsync(
            usuarioId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetarTentativasLogin_CallsRepository()
    {
        var usuarioId = 1;

        await _service.ResetarTentativasLogin(usuarioId, CancellationToken.None);

        await _usuarioRepository.Received(1).ResetarTentativasLoginAsync(
            usuarioId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_HashesPassword()
    {
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@test.com",
            Login = "testuser",
            Senha = "Password123",
            PerfilId = 1
        };

        var validacao = new RegistroValidacaoQuery
        {
            ParceiroExiste = true,
            LoginExiste = false
        };

        _usuarioRepository.ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            Arg.Any<CancellationToken>()).Returns(validacao);

        _usuarioRepository.InserirAsync(
            Arg.Any<UsuarioCommand>(),
            Arg.Any<CancellationToken>()).Returns(1);

        await _service.RegisterAsync(request, CancellationToken.None);

        await _usuarioRepository.Received(1).InserirAsync(
            Arg.Is<UsuarioCommand>(u =>
                u.Senha != request.Senha &&
                BCrypt.Net.BCrypt.Verify(request.Senha, u.Senha)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListarAsync_PassesCancellationTokenToRepository()
    {
        var usuarios = new List<UsuarioComPerfilQuery>
        {
            new UsuarioComPerfilQuery { Id = 1, Nome = "Usuario 1" }
        };
        var cancellationToken = new CancellationToken();
        _usuarioRepository.ListarAsync(_testTenantId, cancellationToken).Returns(usuarios);

        await _service.ListarAsync(cancellationToken);

        await _usuarioRepository.Received(1).ListarAsync(_testTenantId, cancellationToken);
    }

    [Fact]
    public async Task RegisterAsync_PassesCancellationTokenToRepository()
    {
        var request = new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@test.com",
            Login = "testuser",
            Senha = "Password123",
            PerfilId = 1
        };

        var validacao = new RegistroValidacaoQuery
        {
            ParceiroExiste = true,
            LoginExiste = false
        };

        var cancellationToken = new CancellationToken();

        _usuarioRepository.ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            cancellationToken).Returns(validacao);

        _usuarioRepository.InserirAsync(
            Arg.Any<UsuarioCommand>(),
            cancellationToken).Returns(1);

        await _service.RegisterAsync(request, cancellationToken);

        await _usuarioRepository.Received(1).ValidarRegistroAsync(
            request.Login,
            _testTenantId,
            request.PerfilId,
            cancellationToken);
        await _usuarioRepository.Received(1).InserirAsync(
            Arg.Any<UsuarioCommand>(),
            cancellationToken);
    }

    [Fact]
    public async Task IncrementarTentativaLogin_PassesCancellationTokenToRepository()
    {
        var usuarioId = 1;
        var cancellationToken = new CancellationToken();

        await _service.IncrementarTentativaLogin(usuarioId, cancellationToken);

        await _usuarioRepository.Received(1).IncrementarTentativaLoginAsync(
            usuarioId,
            cancellationToken);
    }

    [Fact]
    public async Task ResetarTentativasLogin_PassesCancellationTokenToRepository()
    {
        var usuarioId = 1;
        var cancellationToken = new CancellationToken();

        await _service.ResetarTentativasLogin(usuarioId, cancellationToken);

        await _usuarioRepository.Received(1).ResetarTentativasLoginAsync(
            usuarioId,
            cancellationToken);
    }

    [Fact]
    public async Task BloquearUsuarioAsync_UsuarioNotFound_ThrowsNotFoundException()
    {
        var usuarioId = 1;
        _usuarioRepository.ObterPorIdAsync(usuarioId, Arg.Any<CancellationToken>()).Returns((UsuarioQuery?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.BloquearUsuarioAsync(usuarioId, CancellationToken.None));
        Assert.Equal("Usuário não encontrado", exception.Message);
    }

    [Fact]
    public async Task BloquearUsuarioAsync_ValidUsuario_BloqueiaUsuario()
    {
        var usuarioId = 1;
        var usuario = new UsuarioQuery
        {
            Id = usuarioId,
            Nome = "Test User",
            Login = "testuser",
            Bloqueado = false
        };
        _usuarioRepository.ObterPorIdAsync(usuarioId, Arg.Any<CancellationToken>()).Returns(usuario);

        await _service.BloquearUsuarioAsync(usuarioId, CancellationToken.None);

        Assert.True(usuario.Bloqueado);
        await _usuarioRepository.Received(1).AtualizarAsync(
            Arg.Is<UsuarioCommand>(u => u.Id == usuarioId && u.Bloqueado == true),
            Arg.Any<CancellationToken>());
    }
}
