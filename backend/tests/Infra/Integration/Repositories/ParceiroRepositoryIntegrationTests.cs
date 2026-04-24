using Portal.Features.Escopo.Infra;
using Portal.Features.Parceiro.Infra;
using Portal.Features.Usuario.Infra;

namespace sso.repositories;

[Collection("database")]
public sealed class ParceiroRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public ParceiroRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InserirAsync_PersisteParceiro()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new ParceiroRepository(uow);
        var parceiro = new ParceiroCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Acme",
            Descricao = "Descricao",
            Ativo = true
        };

        var id = await repository.InserirAsync(parceiro);
        var stored = await repository.ObterPorIdAsync(id);

        Assert.Equal(parceiro.Nome, stored?.Nome);
    }

    [Fact]
    public async Task ValidarAtualizacaoAsync_DetectaConflitosDeNome()
    {
        await _fixture.ResetAsync();
        var existenteId = Guid.NewGuid();
        var outroId = Guid.NewGuid();

        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Atual', 'desc', true)", new { id = existenteId });
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Duplicado', 'desc', true)", new { id = outroId });

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new ParceiroRepository(uow);

        var (existe, conflito) = await repository.ValidarAtualizacaoAsync(existenteId, "Duplicado");

        Assert.True(existe);
        Assert.True(conflito);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarEscopo()
    {
        await _fixture.ResetAsync();
        var escopoId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('escopoX') RETURNING id");

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new EscopoRepository(uow);

        var escopo = await repo.ObterPorIdAsync(escopoId);
        Assert.NotNull(escopo);
        Assert.Equal("escopoX", escopo!.Nome);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodosEscopos()
    {
        await _fixture.ResetAsync();
        await _fixture.ExecuteAsync("INSERT INTO sso.escopo (nome) VALUES ('escopoA')");
        await _fixture.ExecuteAsync("INSERT INTO sso.escopo (nome) VALUES ('escopoB')");

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new EscopoRepository(uow);

        var escopos = (await repo.ListarAsync()).ToList();
        Assert.True(escopos.Count >= 2);
    }

    [Fact]
    public async Task ExisteNomeAsync_DeveRetornarFalseParaNomeInexistente()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new EscopoRepository(uow);

        var existe = await repo.ExisteNomeAsync("naoexiste");
        Assert.False(existe);
    }

    [Fact]
    public async Task ObterIdsExistentesAsync_DeveRetornarSomenteIdsExistentes()
    {
        await _fixture.ResetAsync();
        var id1 = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('e1') RETURNING id");
        var id2 = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('e2') RETURNING id");

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new EscopoRepository(uow);

        var ids = (await repo.ObterIdsExistentesAsync(new[] { id1, id2, 9999 })).ToList();
        Assert.Contains(id1, ids);
        Assert.Contains(id2, ids);
        Assert.DoesNotContain(9999, ids);
    }

    [Fact]
    public async Task ObterPorNomeAsync_DeveRetornarParceiro()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'ParceiroX', 'desc', true)", new { id = parceiroId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var parceiro = await repo.ObterPorNomeAsync("ParceiroX", default);
        Assert.NotNull(parceiro);
        Assert.Equal("ParceiroX", parceiro!.Nome);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosOsParceiros()
    {
        await _fixture.ResetAsync();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro A', 'desc', true)", new { id = id1 });
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro B', 'desc', false)", new { id = id2 });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var parceiros = (await repo.ObterTodosAsync(null)).ToList();

        Assert.Equal(2, parceiros.Count);
        Assert.Contains(parceiros, p => p.Nome == "Parceiro A");
        Assert.Contains(parceiros, p => p.Nome == "Parceiro B");
    }

    [Fact]
    public async Task ObterTodosAsync_ComFiltroNome_DeveRetornarApenasCorrespondentes()
    {
        await _fixture.ResetAsync();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Alpha', 'desc', true)", new { id = id1 });
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Beta', 'desc', true)", new { id = id2 });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var parceiros = (await repo.ObterTodosAsync("Alpha")).ToList();

        Assert.Single(parceiros);
        Assert.Equal("Alpha", parceiros[0].Nome);
    }

    [Fact]
    public async Task ObterTodosAsync_SemParceiros_DeveRetornarListaVazia()
    {
        await _fixture.ResetAsync();

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var parceiros = (await repo.ObterTodosAsync(null)).ToList();

        Assert.Empty(parceiros);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAlterarDados()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Antigo', 'desc', true)", new { id = parceiroId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var parceiro = new ParceiroCommand()
        {
            Id = parceiroId,
            Nome = "NovoNome",
            Descricao = "NovaDesc",
            Ativo = false
        };
        await repo.AtualizarAsync(parceiro, default);

        var atualizado = await repo.ObterPorIdAsync(parceiroId);
        Assert.Equal("NovoNome", atualizado!.Nome);
        Assert.False(atualizado.Ativo);
    }

    [Fact]
    public async Task ValidarAtualizacaoAsync_DeveRetornarFalseParaNomeUnico()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Unico', 'desc', true)", new { id = parceiroId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new ParceiroRepository(uow);

        var (existe, conflito) = await repo.ValidarAtualizacaoAsync(parceiroId, "OutroNome");
        Assert.True(existe);
        Assert.False(conflito);
    }

    [Fact]
    public async Task Parceiro_ComPerfilEComEscopo_DeveRetornarEscoposRelacionados()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'ParceiroEscopo', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('PerfilEscopo') RETURNING id");
        var escopoId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('escopoTeste') RETURNING id");
        await _fixture.ExecuteAsync("INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId)", new { perfilId, escopoId });
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var authRepo = new AuthRepository(uow);
        var escopos = await authRepo.ObterEscoposDoUsuarioAsync(usuarioId, CancellationToken.None);

        Assert.Contains("escopoTeste", escopos);
    }
}
