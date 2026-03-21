using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Portal.Dominio.Validations;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;
using Portal.Features.Parceiro.Domain.Validations;
using Portal.Features.Parceiro.Service;
using System.Security.Claims;
using FluentValidation.Results;

namespace sso_tests;

public class ParceiroServiceTests
{
    private readonly IParceiroRepository _repository;
    private readonly ParceiroRequestValidator _validator;
    private readonly ParceiroIdRequestValidator _validatorId;
    private readonly ILogger<ParceiroService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ParceiroServiceTests()
    {
        _repository = Substitute.For<IParceiroRepository>();
        _validator = new ParceiroRequestValidator();
        _validatorId = new ParceiroIdRequestValidator();
        _logger = Substitute.For<ILogger<ParceiroService>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        
        // Setup default HttpContext with valid TenantId
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim("tenantId", Guid.NewGuid().ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    private ParceiroService CreateService() => new(
        _repository,
        _validator,
        _validatorId,
        _logger,
        _httpContextAccessor);

    [Fact]
    public void Constructor_InitializesAllDependencies()
    {
        var service = CreateService();

        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListarParceirosAsync_WithNoResults_ThrowsNotFoundException()
    {
        var service = CreateService();
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ParceiroResponse>());

        await Assert.ThrowsAsync<NotFoundException>(() => 
            service.ListarParceirosAsync(null, CancellationToken.None));
    }

    [Fact]
    public async Task ListarParceirosAsync_WithNullResult_ThrowsNotFoundException()
    {
        var service = CreateService();
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ParceiroResponse>>(null!));

        await Assert.ThrowsAsync<NotFoundException>(() => 
            service.ListarParceirosAsync("test", CancellationToken.None));
    }

    [Fact]
    public async Task ListarParceirosAsync_WithResults_ReturnsAllParceiros()
    {
        var service = CreateService();
        var parceiros = new List<ParceiroResponse>
        {
            new() { Id = Guid.NewGuid(), Nome = "Parceiro 1", Ativo = true },
            new() { Id = Guid.NewGuid(), Nome = "Parceiro 2", Ativo = true }
        };
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(parceiros);

        var result = await service.ListarParceirosAsync(null, CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ListarParceirosAsync_WithNameFilter_PassesFilterToRepository()
    {
        var service = CreateService();
        var parceiros = new List<ParceiroResponse>
        {
            new() { Id = Guid.NewGuid(), Nome = "Filtered", Ativo = true }
        };
        _repository.ObterTodosAsync("Filtered", Arg.Any<CancellationToken>())
            .Returns(parceiros);

        var result = await service.ListarParceirosAsync("Filtered", CancellationToken.None);

        await _repository.Received(1).ObterTodosAsync("Filtered", Arg.Any<CancellationToken>());
        Assert.Single(result);
    }

    [Fact]
    public async Task ObterParceiroAsync_WithNullId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.ObterParceiroAsync(null, CancellationToken.None));
    }

    [Fact]
    public async Task ObterParceiroAsync_WithEmptyId_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.ObterParceiroAsync(string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task ObterParceiroAsync_WithInvalidGuid_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.ObterParceiroAsync("invalid-guid", CancellationToken.None));
    }

    [Fact]
    public async Task ObterParceiroAsync_WithEmptyGuid_ThrowsValidationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.ObterParceiroAsync(Guid.Empty.ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task ObterParceiroAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((ParceiroResponse?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            service.ObterParceiroAsync(id.ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task ObterParceiroAsync_WhenFound_ReturnsParceiro()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var parceiro = new ParceiroResponse
        {
            Id = id,
            Nome = "Test Parceiro",
            Descricao = "Test Description",
            Ativo = true
        };
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(parceiro);

        var result = await service.ObterParceiroAsync(id.ToString(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test Parceiro", result.Nome);
    }

    [Fact]
    public async Task CriarParceiroAsync_WithInvalidRequest_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new ParceiroRequest { Nome = string.Empty }; // Invalid: empty name

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.CriarParceiroAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CriarParceiroAsync_WithDuplicateName_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new ParceiroRequest { Nome = "Existing Parceiro", Descricao = "Test" };
        var existingParceiro = new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Existing Parceiro" };
        _repository.ObterPorNomeAsync("Existing Parceiro", Arg.Any<CancellationToken>())
            .Returns(existingParceiro);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => 
            service.CriarParceiroAsync(request, CancellationToken.None));

        Assert.Contains("Já existe um parceiro com este nome.", ex.Errors);
    }

    [Fact]
    public async Task CriarParceiroAsync_WithValidRequest_CreatesAndReturnsId()
    {
        var service = CreateService();
        var newId = Guid.NewGuid();
        var request = new ParceiroRequest { Nome = "New Parceiro", Descricao = "Description" };
        _repository.ObterPorNomeAsync(request.Nome, Arg.Any<CancellationToken>())
            .Returns((ParceiroResponse?)null);
        _repository.InserirAsync(Arg.Any<Portal.Domain.Entities.ParceiroEntity>(), Arg.Any<CancellationToken>())
            .Returns(newId);

        var result = await service.CriarParceiroAsync(request, CancellationToken.None);

        Assert.Equal(newId, result);
        await _repository.Received(1).InserirAsync(
            Arg.Is<Portal.Domain.Entities.ParceiroEntity>(e => e.Nome == request.Nome),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarParceiroAsync_WithInvalidRequest_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new AtualizarParceiroRequest 
        { 
            Id = Guid.NewGuid().ToString(),
            Nome = string.Empty // Invalid: empty name
        };

        await Assert.ThrowsAsync<ValidationException>(() => 
            service.AtualizarParceiroAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task AtualizarParceiroAsync_WhenParceiroNotExists_ThrowsNotFoundException()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(),
            Nome = "Updated Name",
            Descricao = "Updated Desc",
            Ativo = true
        };
        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((false, false)); // doesn't exist

        await Assert.ThrowsAsync<NotFoundException>(() => 
            service.AtualizarParceiroAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task AtualizarParceiroAsync_WhenNameConflict_ThrowsValidationException()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(),
            Nome = "Conflicting Name",
            Descricao = "Desc",
            Ativo = true
        };
        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((true, true)); // exists but name conflicts

        var ex = await Assert.ThrowsAsync<ValidationException>(() => 
            service.AtualizarParceiroAsync(request, CancellationToken.None));

        Assert.Contains("Já existe outro parceiro com este nome.", ex.Errors);
    }

    [Fact]
    public async Task AtualizarParceiroAsync_WithValidRequest_UpdatesParceiro()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(),
            Nome = "Updated Name",
            Descricao = "Updated Description",
            Ativo = false
        };
        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((true, false)); // exists and no conflict

        await service.AtualizarParceiroAsync(request, CancellationToken.None);

        await _repository.Received(1).AtualizarAsync(
            Arg.Is<Portal.Domain.Entities.ParceiroEntity>(e => 
                e.Id == id && 
                e.Nome == request.Nome && 
                e.Descricao == request.Descricao &&
                e.Ativo == request.Ativo),
            Arg.Any<CancellationToken>());
    }
}
