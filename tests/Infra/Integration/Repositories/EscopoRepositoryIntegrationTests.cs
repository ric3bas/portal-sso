using Portal.Dominio.Entities;
using Portal.Features.Escopo.Infra;

namespace sso.repositories;

[Collection("database")]
public sealed class EscopoRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public EscopoRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InserirAsync_E_ObterIdsExistentes()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new EscopoRepository(uow);

        var id = await repository.InserirAsync(new EscopoEntity { Nome = "finance" });
        var ids = (await repository.ObterIdsExistentesAsync(new[] { id, 99 })).ToList();

        Assert.Single(ids);
        Assert.Equal(id, ids[0]);
    }

    [Fact]
    public async Task ExisteNomeAsync_DeveDetectarDuplicidade()
    {
        await _fixture.ResetAsync();
        await _fixture.ExecuteAsync("INSERT INTO sso.escopo (nome) VALUES ('read')");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new EscopoRepository(uow);

        var existe = await repository.ExisteNomeAsync("READ");

        Assert.True(existe);
    }
}
