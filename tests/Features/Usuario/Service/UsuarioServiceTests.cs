using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Service;
using Portal.Infra;

namespace sso.services;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPerfilRepository _perfilRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioServiceTests()
    {
        _usuarioRepository = Substitute.For<IUsuarioRepository>();
        _perfilRepository  = Substitute.For<IPerfilRepository>();
        _unitOfWork        = Substitute.For<IUnitOfWork>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    private UsuarioService CreateService()
        => new UsuarioService(_usuarioRepository, _perfilRepository, _unitOfWork, _httpContextAccessor);

    [Fact]
    public void Constructor_InitializesService()
    {
        var service = CreateService();

        Assert.NotNull(service);
    }

    [Fact]
    public void ListarComPerfisAsync_ThrowsNotImplementedException()
    {
        var service = CreateService();

        var exception = Assert.ThrowsAsync<NotImplementedException>(
            async () => await service.ListarComPerfisAsync());

        Assert.NotNull(exception);
    }

    [Fact]
    public void ListarComPerfisAsync_WithCancellationToken_ThrowsNotImplementedException()
    {
        var service = CreateService();
        var cancellationToken = new CancellationToken();

        var exception = Assert.ThrowsAsync<NotImplementedException>(
            async () => await service.ListarComPerfisAsync(cancellationToken));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRequest_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new RegisterRequest();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await service.RegisterAsync(request));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task RegisterAsync_ParceiroNotFound_ThrowsNotFoundException()
    {
        SetupHttpContext(Guid.NewGuid());
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = false,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await service.RegisterAsync(request));

        Assert.Contains("não encontrado", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_LoginAlreadyExists_ThrowsValidationException()
    {
        SetupHttpContext(Guid.NewGuid());
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = true,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await service.RegisterAsync(request));

        Assert.Contains("já existe", exception.Errors[0]);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_InsertsUser()
    {
        var parceiroId = Guid.NewGuid();
        SetupHttpContext(parceiroId);
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);
        _usuarioRepository.InserirAsync(Arg.Any<UsuarioEntity>()).Returns(1);

        await service.RegisterAsync(request);

        await _usuarioRepository.Received(1).InserirAsync(Arg.Is<UsuarioEntity>(u =>
            u.Nome == request.Nome &&
            u.Email == request.Email &&
            u.Login == request.Login &&
            u.ParceiroId == parceiroId &&
            u.PerfilId == request.PerfilId));
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_HashesPassword()
    {
        SetupHttpContext(Guid.NewGuid());
        var service = CreateService();
        var request = CreateValidRequest();
        request.Senha = "plainPassword123";
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);
        _usuarioRepository.InserirAsync(Arg.Any<UsuarioEntity>()).Returns(1);

        await service.RegisterAsync(request);

        await _usuarioRepository.Received(1).InserirAsync(Arg.Is<UsuarioEntity>(u =>
            u.Senha != request.Senha &&
            BCrypt.Net.BCrypt.Verify(request.Senha, u.Senha)));
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CallsUnitOfWork()
    {
        SetupHttpContext(Guid.NewGuid());
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);
        _usuarioRepository.InserirAsync(Arg.Any<UsuarioEntity>()).Returns(1);

        await service.RegisterAsync(request);

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Commit();
        _unitOfWork.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task RegisterAsync_InsertFails_RollsBackTransaction()
    {
        SetupHttpContext(Guid.NewGuid());
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);
        _usuarioRepository.InserirAsync(Arg.Any<UsuarioEntity>())
            .Returns<Task<int>>(x => throw new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.RegisterAsync(request));

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CallsValidarRegistroAsync()
    {
        var parceiroId = Guid.NewGuid();
        SetupHttpContext(parceiroId);
        var service = CreateService();
        var request = CreateValidRequest();
        var validacao = new RegistroValidacao
        {
            ParceiroExiste = true,
            LoginExiste = false,
            PerfilExiste = true
        };
        _usuarioRepository.ValidarRegistroAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(validacao);
        _usuarioRepository.InserirAsync(Arg.Any<UsuarioEntity>()).Returns(1);

        await service.RegisterAsync(request);

        await _usuarioRepository.Received(1).ValidarRegistroAsync(
            request.Login,
            parceiroId,
            request.PerfilId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListarAsync_WithoutResults_ThrowsNotFoundException()
    {
        var parceiroId = Guid.NewGuid();
        SetupHttpContext(parceiroId);
        var service = CreateService();
        _usuarioRepository.ListarAsync(parceiroId, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<UsuarioComPerfilResponse>());

        await Assert.ThrowsAsync<NotFoundException>(() => service.ListarAsync());
    }

    [Fact]
    public async Task ListarAsync_WithResults_ReturnsUsuarios()
    {
        var parceiroId = Guid.NewGuid();
        SetupHttpContext(parceiroId);
        var service = CreateService();
        var usuarios = new List<UsuarioComPerfilResponse>
        {
            new() { Id = 1, Nome = "User 1", Login = "user1", Email = "u1@teste.com" }
        };

        _usuarioRepository.ListarAsync(parceiroId, Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = await service.ListarAsync();

        Assert.Single(result);
    }

    private void SetupHttpContext(Guid tenantId)
    {
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("tenantId", tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContext.User = claimsPrincipal;
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    private RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest
        {
            Nome = "Test User",
            Email = "test@example.com",
            Login = "testuser",
            Senha = "Password123!",
            PerfilId = 1
        };
    }
}
