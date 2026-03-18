using Microsoft.Extensions.Logging;
using NSubstitute;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Escopo.Service;

namespace sso.services;

public class EscopoServiceTests
{
    private readonly IEscopoRepository _repository;
    private readonly ILogger<EscopoService> _logger;

    public EscopoServiceTests()
    {
        _repository = Substitute.For<IEscopoRepository>();
        _logger = Substitute.For<ILogger<EscopoService>>();
    }

    private EscopoService CreateService() => new(_repository, _logger);

    [Fact]
    public async Task ListarAsync_WithoutResults_ThrowsNotFoundException()
    {
        var service = CreateService();
        _repository.ListarAsync(Arg.Any<CancellationToken>()).Returns(Enumerable.Empty<EscopoResponse>());

        await Assert.ThrowsAsync<NotFoundException>(() => service.ListarAsync());
    }

    [Fact]
    public async Task ListarAsync_WithResults_ReturnsEscopos()
    {
        var service = CreateService();
        var escopos = new List<EscopoResponse>
        {
            new() { Id = 1, Nome = "read" },
            new() { Id = 2, Nome = "write" }
        };
        _repository.ListarAsync(Arg.Any<CancellationToken>()).Returns(escopos);

        var result = await service.ListarAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CriarAsync_WithEmptyNome_ThrowsValidationException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CriarAsync(" "));

        Assert.Contains("obrigatório", ex.Errors[0]);
    }

    [Fact]
    public async Task CriarAsync_WithShortNome_ThrowsValidationException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CriarAsync("ab"));

        Assert.Contains("no mínimo 3", ex.Errors[0]);
    }

    [Fact]
    public async Task CriarAsync_WithLongNome_ThrowsValidationException()
    {
        var service = CreateService();
        var nome = new string('a', 101);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CriarAsync(nome));

        Assert.Contains("no máximo 100", ex.Errors[0]);
    }

    [Fact]
    public async Task CriarAsync_WithDuplicatedNome_ThrowsBusinessException()
    {
        var service = CreateService();
        _repository.ExisteNomeAsync("admin", Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.CriarAsync("admin"));

        Assert.Contains("Já existe", ex.Errors[0]);
    }

    [Fact]
    public async Task CriarAsync_WithValidNome_TrimsAndInserts()
    {
        var service = CreateService();
        _repository.ExisteNomeAsync("admin", Arg.Any<CancellationToken>()).Returns(false);
        _repository.InserirAsync(Arg.Any<Escopo>(), Arg.Any<CancellationToken>()).Returns(7);

        var id = await service.CriarAsync("  admin  ");

        Assert.Equal(7, id);
        await _repository.Received(1).InserirAsync(Arg.Is<Escopo>(x => x.Nome == "admin"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_WithInvalidId_ThrowsValidationException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.ObterPorIdAsync(0));

        Assert.Contains("inválido", ex.Errors[0]);
    }

    [Fact]
    public async Task ObterPorIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var service = CreateService();
        _repository.ObterPorIdAsync(5, Arg.Any<CancellationToken>()).Returns((Escopo?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ObterPorIdAsync(5));
    }

    [Fact]
    public async Task ObterPorIdAsync_WhenFound_ReturnsEscopo()
    {
        var service = CreateService();
        var escopo = new Escopo { Id = 10, Nome = "manage" };
        _repository.ObterPorIdAsync(10, Arg.Any<CancellationToken>()).Returns(escopo);

        var result = await service.ObterPorIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("manage", result.Nome);
    }
}
