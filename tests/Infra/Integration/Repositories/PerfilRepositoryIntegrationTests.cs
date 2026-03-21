using Dapper;
using Npgsql;
using Portal.Dominio.Entities;
using Portal.Features.Perfil.Infra;

namespace sso.repositories;

[Collection("database")]
public sealed class PerfilRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public PerfilRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListarComEscoposAsync_DeveAgruparEscoposPorPerfil()
    {
        await _fixture.ResetAsync();
        var perfilId = await CriarPerfilComEscoposAsync("PerfilAggregado", new[] { "manage", "read" });

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        var result = await repository.ListarComEscoposAsync();

        Assert.Contains(result, perfil => perfil.Id == perfilId && perfil.Escopos.Count == 2);
    }

    [Fact]
    public async Task InserirAsync_DeveRetornarIdDoPerfil()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        var perfil = new PerfilEntity { Nome = "Novo Perfil" };
        var id = await repository.InserirAsync(perfil);

        Assert.True(id > 0);

        var existente = await repository.ObterPorIdAsync(id);
        Assert.NotNull(existente);
        Assert.Equal("Novo Perfil", existente!.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPerfilQuandoExistir()
    {
        await _fixture.ResetAsync();
        var perfilId = await CriarPerfilAsync("PerfilPorId");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        var perfil = await repository.ObterPorIdAsync(perfilId);

        Assert.NotNull(perfil);
        Assert.Equal(perfilId, perfil!.Id);
    }

    [Fact]
    public async Task ExistePerfilAsync_DeveRetornarTrueQuandoExiste()
    {
        await _fixture.ResetAsync();
        var perfilId = await CriarPerfilAsync("PerfilExiste");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        var existe = await repository.ExistePerfilAsync(perfilId);

        Assert.True(existe);
    }

    [Fact]
    public async Task ExisteNomeAsync_DeveSerCaseInsensitive()
    {
        await _fixture.ResetAsync();
        await CriarPerfilAsync("PerfilCase");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        var existe = await repository.ExisteNomeAsync("perfilcase");

        Assert.True(existe);
    }

    [Fact]
    public async Task VincularEscoposAsync_RegistraEscoposParaPerfil()
    {
        await _fixture.ResetAsync();
        var perfilId = await CriarPerfilAsync("PerfilVinculado");
        var escopoIds = new List<int>
        {
            await CriarEscopoAsync("escopo-1"),
            await CriarEscopoAsync("escopo-2")
        };

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new PerfilRepository(uow);

        await repository.VincularEscoposAsync(perfilId, escopoIds);

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        var vinculados = await connection.QueryAsync<int>(
            "SELECT escopo_id FROM sso.perfil_escopo WHERE perfil_id = @perfilId",
            new { perfilId });

        Assert.Equal(escopoIds.Count, vinculados.Count());
        Assert.All(escopoIds, id => Assert.Contains(id, vinculados));
    }

    private async Task<int> CriarPerfilAsync(string nome)
    {
        await _fixture.ExecuteAsync("INSERT INTO sso.perfil (nome) VALUES (@nome)", new { nome });
        return await _fixture.ExecuteScalarAsync("SELECT id FROM sso.perfil WHERE nome = @nome", new { nome });
    }

    private async Task<int> CriarEscopoAsync(string nome)
    {
        await _fixture.ExecuteAsync("INSERT INTO sso.escopo (nome) VALUES (@nome)", new { nome });
        return await _fixture.ExecuteScalarAsync("SELECT id FROM sso.escopo WHERE nome = @nome", new { nome });
    }

    private async Task<int> CriarPerfilComEscoposAsync(string nome, IEnumerable<string> escopos)
    {
        var perfilId = await CriarPerfilAsync(nome);
        foreach (var escopo in escopos)
        {
            var escopoId = await CriarEscopoAsync(escopo);
            await _fixture.ExecuteAsync("INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId)", new { perfilId, escopoId });
        }

        return perfilId;
    }
}

