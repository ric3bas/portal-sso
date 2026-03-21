using Microsoft.Extensions.Logging;
using NSubstitute;
using Portal.Dominio.Validations;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Perfil.Domain;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Perfil.Service;
using PerfilEntity = Portal.Dominio.Entities.PerfilEntity;

namespace sso.services;

public class PerfilServiceTests
{
    private readonly IPerfilRepository _repository;
    private readonly IEscopoRepository _escopoRepository;
    private readonly ILogger<PerfilService> _logger;
    private readonly PerfilService _service;

    public PerfilServiceTests()
    {
        _repository = Substitute.For<IPerfilRepository>();
        _escopoRepository = Substitute.For<IEscopoRepository>();
        _logger = Substitute.For<ILogger<PerfilService>>();
        
        _service = new PerfilService(_repository, _escopoRepository, _logger);
    }

    [Fact]
    public void Constructor_InitializesAllDependencies()
    {
        // Arrange
        var repository = Substitute.For<IPerfilRepository>();
        var escopoRepository = Substitute.For<IEscopoRepository>();
        var logger = Substitute.For<ILogger<PerfilService>>();

        // Act
        var service = new PerfilService(repository, escopoRepository, logger);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListarComEscoposAsync_WithResults_ReturnsResults()
    {
        // Arrange
        var perfis = new List<PerfilComEscopoResponse>
        {
            new PerfilComEscopoResponse { Id = 1, Nome = "Perfil 1" },
            new PerfilComEscopoResponse { Id = 2, Nome = "Perfil 2" }
        };
        _repository.ListarComEscoposAsync(Arg.Any<CancellationToken>()).Returns(perfis);

        // Act
        var result = await _service.ListarComEscoposAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ListarComEscoposAsync_NoResults_ThrowsNotFoundException()
    {
        // Arrange
        var perfis = new List<PerfilComEscopoResponse>();
        _repository.ListarComEscoposAsync(Arg.Any<CancellationToken>()).Returns(perfis);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ListarComEscoposAsync(CancellationToken.None));
        Assert.Equal("Nenhum perfil encontrado", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_ValidNome_ReturnsId()
    {
        // Arrange
        var nome = "Perfil Teste";
        var expectedId = 1;
        _repository.ExisteNomeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.InserirAsync(Arg.Any<PerfilEntity>(), Arg.Any<CancellationToken>()).Returns(expectedId);

        // Act
        var result = await _service.CriarAsync(nome, CancellationToken.None);

        // Assert
        Assert.Equal(expectedId, result);
        await _repository.Received(1).InserirAsync(
            Arg.Is<PerfilEntity>(p => p.Nome == nome.Trim()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_NullNome_ThrowsValidationException()
    {
        // Arrange
        string? nome = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CriarAsync(nome!, CancellationToken.None));
        Assert.Contains("Nome do perfil é obrigatório", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_EmptyNome_ThrowsValidationException()
    {
        // Arrange
        var nome = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CriarAsync(nome, CancellationToken.None));
        Assert.Contains("Nome do perfil é obrigatório", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_WhitespaceNome_ThrowsValidationException()
    {
        // Arrange
        var nome = "   ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CriarAsync(nome, CancellationToken.None));
        Assert.Contains("Nome do perfil é obrigatório", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_NomeTooShort_ThrowsValidationException()
    {
        // Arrange
        var nome = "ab";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CriarAsync(nome, CancellationToken.None));
        Assert.Contains("Nome do perfil deve ter no mínimo 3 caracteres", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_NomeTooLong_ThrowsValidationException()
    {
        // Arrange
        var nome = new string('a', 101);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CriarAsync(nome, CancellationToken.None));
        Assert.Contains("Nome do perfil deve ter no máximo 100 caracteres", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_ExistingNome_ThrowsBusinessException()
    {
        // Arrange
        var nome = "Perfil Existente";
        _repository.ExisteNomeAsync(nome.Trim(), Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _service.CriarAsync(nome, CancellationToken.None));
        Assert.Contains($"Já existe um perfil com o nome '{nome}'", exception.Errors);
    }

    [Fact]
    public async Task CriarAsync_NomeWithLeadingTrailingSpaces_TrimsName()
    {
        // Arrange
        var nome = "  Perfil Teste  ";
        var expectedId = 1;
        _repository.ExisteNomeAsync("Perfil Teste", Arg.Any<CancellationToken>()).Returns(false);
        _repository.InserirAsync(Arg.Any<PerfilEntity>(), Arg.Any<CancellationToken>()).Returns(expectedId);

        // Act
        var result = await _service.CriarAsync(nome, CancellationToken.None);

        // Assert
        Assert.Equal(expectedId, result);
        await _repository.Received(1).ExisteNomeAsync("Perfil Teste", Arg.Any<CancellationToken>());
        await _repository.Received(1).InserirAsync(
            Arg.Is<PerfilEntity>(p => p.Nome == "Perfil Teste"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_ValidId_ReturnsPerfil()
    {
        // Arrange
        var id = 1;
        var perfil = new PerfilEntity { Id = id, Nome = "Perfil Teste" };
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(perfil);

        // Act
        var result = await _service.ObterPorIdAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Perfil Teste", result.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_InvalidId_ThrowsValidationException()
    {
        // Arrange
        var id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ObterPorIdAsync(id, CancellationToken.None));
        Assert.Contains("Id do perfil inválido", exception.Errors);
    }

    [Fact]
    public async Task ObterPorIdAsync_NegativeId_ThrowsValidationException()
    {
        // Arrange
        var id = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ObterPorIdAsync(id, CancellationToken.None));
        Assert.Contains("Id do perfil inválido", exception.Errors);
    }

    [Fact]
    public async Task ObterPorIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((PerfilEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ObterPorIdAsync(id, CancellationToken.None));
        Assert.Equal($"Perfil {id} não encontrado", exception.Message);
    }

    [Fact]
    public async Task VincularEscoposAsync_ValidData_CallsRepository()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int> { 1, 2, 3 };
        _repository.ExistePerfilAsync(perfilId, Arg.Any<CancellationToken>()).Returns(true);
        _escopoRepository.ObterIdsExistentesAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(escopoIds);

        // Act
        await _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None);

        // Assert
        await _repository.Received(1).VincularEscoposAsync(
            perfilId,
            Arg.Is<List<int>>(ids => ids.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularEscoposAsync_InvalidPerfilId_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 0;
        var escopoIds = new List<int> { 1, 2 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("PerfilId inválido", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_NegativePerfilId_ThrowsValidationException()
    {
        // Arrange
        var perfilId = -1;
        var escopoIds = new List<int> { 1, 2 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("PerfilId inválido", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_NullEscopoIds_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 1;
        IEnumerable<int>? escopoIds = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds!, CancellationToken.None));
        Assert.Contains("Informe ao menos um EscopoId", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_EmptyEscopoIds_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("Informe ao menos um EscopoId", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_InvalidEscopoId_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int> { 1, 0, 3 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("Todos os EscopoIds devem ser maiores que zero", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_NegativeEscopoId_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int> { 1, -1, 3 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("Todos os EscopoIds devem ser maiores que zero", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_PerfilNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var perfilId = 99;
        var escopoIds = new List<int> { 1, 2, 3 };
        _repository.ExistePerfilAsync(perfilId, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Equal($"Perfil {perfilId} não encontrado", exception.Message);
    }

    [Fact]
    public async Task VincularEscoposAsync_NonExistentEscopoIds_ThrowsValidationException()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int> { 1, 2, 3, 4 };
        var existingIds = new List<int> { 1, 3 };
        _repository.ExistePerfilAsync(perfilId, Arg.Any<CancellationToken>()).Returns(true);
        _escopoRepository.ObterIdsExistentesAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(existingIds);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None));
        Assert.Contains("Os seguintes EscopoIds não existem: 2, 4", exception.Errors);
    }

    [Fact]
    public async Task VincularEscoposAsync_DuplicateEscopoIds_RemovesDuplicates()
    {
        // Arrange
        var perfilId = 1;
        var escopoIds = new List<int> { 1, 2, 2, 3, 3 };
        var expectedIds = new List<int> { 1, 2, 3 };
        _repository.ExistePerfilAsync(perfilId, Arg.Any<CancellationToken>()).Returns(true);
        _escopoRepository.ObterIdsExistentesAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(expectedIds);

        // Act
        await _service.VincularEscoposAsync(perfilId, escopoIds, CancellationToken.None);

        // Assert
        await _repository.Received(1).VincularEscoposAsync(
            perfilId,
            Arg.Is<List<int>>(ids => ids.Count == 3 && ids.Contains(1) && ids.Contains(2) && ids.Contains(3)),
            Arg.Any<CancellationToken>());
    }
}
