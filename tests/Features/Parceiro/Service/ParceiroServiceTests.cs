using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Portal.Dominio.Validations;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;
using Portal.Features.Parceiro.Service;
using Portal.Infra;
using System.Security.Claims;

namespace sso.services;

public class ParceiroServiceTests
{
    private readonly IParceiroRepository _repository;
    private readonly Portal.Features.Parceiro.Domain.Validations.ParceiroRequestValidator _validator;
    private readonly Portal.Features.Parceiro.Domain.Validations.ParceiroIdRequestValidator _validatorId;
    private readonly ILogger<ParceiroService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ParceiroService _service;

    public ParceiroServiceTests()
    {
        _repository = Substitute.For<IParceiroRepository>();
        _validator = new Portal.Features.Parceiro.Domain.Validations.ParceiroRequestValidator();
        _validatorId = new Portal.Features.Parceiro.Domain.Validations.ParceiroIdRequestValidator();
        _logger = Substitute.For<ILogger<ParceiroService>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        
        _service = new ParceiroService(
            _repository,
            _validator,
            _validatorId,
            _logger,
            _unitOfWork,
            _httpContextAccessor);
    }

    [Fact]
    public void Constructor_InitializesAllDependencies()
    {
        // Arrange
        var repository = Substitute.For<IParceiroRepository>();
        var validator = new Portal.Features.Parceiro.Domain.Validations.ParceiroRequestValidator();
        var validatorId = new Portal.Features.Parceiro.Domain.Validations.ParceiroIdRequestValidator();
        var logger = Substitute.For<ILogger<ParceiroService>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        // Act
        var service = new ParceiroService(
            repository,
            validator,
            validatorId,
            logger,
            unitOfWork,
            httpContextAccessor);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListarParceirosAsync_WithResults_ReturnsResults()
    {
        // Arrange
        var parceiros = new List<ParceiroResponse>
        {
            new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Parceiro 1", Ativo = true },
            new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Parceiro 2", Ativo = true }
        };
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(parceiros);

        // Act
        var result = await _service.ListarParceirosAsync("test", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ListarParceirosAsync_NoResults_ThrowsNotFoundException()
    {
        // Arrange
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ParceiroResponse>>(null!));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ListarParceirosAsync("test", CancellationToken.None));
        Assert.Equal("Nenhum parceiro encontrado", exception.Message);
    }

    [Fact]
    public async Task ListarParceirosAsync_EmptyResults_ThrowsNotFoundException()
    {
        // Arrange
        _repository.ObterTodosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ParceiroResponse>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ListarParceirosAsync("test", CancellationToken.None));
        Assert.Equal("Nenhum parceiro encontrado", exception.Message);
    }

    [Fact]
    public async Task ObterParceiroAsync_ValidId_ReturnsParceiro()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parceiro = new ParceiroResponse { Id = id, Nome = "Test", Ativo = true };
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(parceiro);

        // Act
        var result = await _service.ObterParceiroAsync(id.ToString(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task ObterParceiroAsync_InvalidId_ThrowsValidationException()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Portal.Dominio.Validations.ValidationException>(
            () => _service.ObterParceiroAsync(invalidId, CancellationToken.None));
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public async Task ObterParceiroAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((ParceiroResponse?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ObterParceiroAsync(id.ToString(), CancellationToken.None));
        Assert.Equal("Parceiro não encontrado", exception.Message);
    }

    [Fact]
    public async Task CriarParceiroAsync_ValidRequest_ReturnsNewId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupHttpContext(tenantId);

        var request = new ParceiroRequest { Nome = "Novo Parceiro", Descricao = "Descrição" };
        var newId = Guid.NewGuid();

        _repository.ObterPorNomeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ParceiroResponse?)null);
        _repository.InserirAsync(Arg.Any<Portal.Domain.Entities.ParceiroEntity>(), Arg.Any<CancellationToken>())
            .Returns(newId);

        // Act
        var result = await _service.CriarParceiroAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(newId, result);
        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Commit();
    }

    [Fact]
    public async Task CriarParceiroAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new ParceiroRequest { Nome = null, Descricao = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Portal.Dominio.Validations.ValidationException>(
            () => _service.CriarParceiroAsync(request, CancellationToken.None));
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public async Task CriarParceiroAsync_DuplicateName_ThrowsValidationException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupHttpContext(tenantId);

        var request = new ParceiroRequest { Nome = "Parceiro Existente", Descricao = "Test" };
        var existingParceiro = new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Parceiro Existente", Ativo = true };

        _repository.ObterPorNomeAsync(request.Nome, Arg.Any<CancellationToken>())
            .Returns(existingParceiro);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Portal.Dominio.Validations.ValidationException>(
            () => _service.CriarParceiroAsync(request, CancellationToken.None));
        Assert.Contains("Já existe um parceiro com este nome.", exception.Errors);
    }

    [Fact]
    public async Task CriarParceiroAsync_RepositoryException_RollsBackTransaction()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupHttpContext(tenantId);

        var request = new ParceiroRequest { Nome = "Test", Descricao = "Test" };

        _repository.ObterPorNomeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ParceiroResponse?)null);
        _repository.InserirAsync(Arg.Any<Portal.Domain.Entities.ParceiroEntity>(), Arg.Any<CancellationToken>())
            .Returns<Guid>(x => throw new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _service.CriarParceiroAsync(request, CancellationToken.None));

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
    }

    [Fact]
    public async Task AtualizarParceiroAsync_ValidRequest_UpdatesParceiro()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(), 
            Nome = "Updated", 
            Descricao = "Updated Description", 
            Ativo = true 
        };

        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((true, false));

        // Act
        await _service.AtualizarParceiroAsync(request, CancellationToken.None);

        // Assert
        await _repository.Received(1).AtualizarAsync(Arg.Any<Portal.Domain.Entities.ParceiroEntity>(), Arg.Any<CancellationToken>());
        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Commit();
    }

    [Fact]
    public async Task AtualizarParceiroAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new AtualizarParceiroRequest 
        { 
            Id = Guid.NewGuid().ToString(), 
            Nome = "", 
            Descricao = "Test", 
            Ativo = true 
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Portal.Dominio.Validations.ValidationException>(
            () => _service.AtualizarParceiroAsync(request, CancellationToken.None));
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public async Task AtualizarParceiroAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(), 
            Nome = "Test", 
            Descricao = "Test", 
            Ativo = true 
        };

        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((false, false));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.AtualizarParceiroAsync(request, CancellationToken.None));
        Assert.Equal("Parceiro não encontrado", exception.Message);
    }

    [Fact]
    public async Task AtualizarParceiroAsync_DuplicateName_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(), 
            Nome = "Existing Name", 
            Descricao = "Test", 
            Ativo = true 
        };

        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((true, true));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Portal.Dominio.Validations.ValidationException>(
            () => _service.AtualizarParceiroAsync(request, CancellationToken.None));
        Assert.Contains("Já existe outro parceiro com este nome.", exception.Errors);
    }

    [Fact]
    public async Task AtualizarParceiroAsync_RepositoryException_RollsBackTransaction()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarParceiroRequest 
        { 
            Id = id.ToString(), 
            Nome = "Test", 
            Descricao = "Test", 
            Ativo = true 
        };

        _repository.ValidarAtualizacaoAsync(id, request.Nome, Arg.Any<CancellationToken>())
            .Returns((true, false));
        _repository.AtualizarAsync(Arg.Any<Portal.Domain.Entities.ParceiroEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Database error")));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _service.AtualizarParceiroAsync(request, CancellationToken.None));

        _unitOfWork.Received(1).Begin();
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
    }

    private void SetupHttpContext(Guid tenantId)
    {
        var httpContext = Substitute.For<HttpContext>();
        var claims = new List<Claim>
        {
            new Claim("tenantId", tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContext.User.Returns(claimsPrincipal);
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }
}
