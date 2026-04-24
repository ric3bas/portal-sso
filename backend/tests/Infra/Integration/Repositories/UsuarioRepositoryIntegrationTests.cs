using Portal.Features.Usuario.Infra;

namespace sso.repositories;

[Collection("database")]
public sealed class UsuarioRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public UsuarioRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InserirAsync_E_PesquisarUsuario()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new UsuarioRepository(uow);
        var entity = new UsuarioCommand
        {
            Nome = "UsuÃ¡rio",
            Email = "user@teste.com",
            Login = "user-teste",
            Senha = "hash",
            ParceiroId = parceiroId,
            PerfilId = perfilId
        };

        var id = await repository.InserirAsync(entity, CancellationToken.None);
        var usuarios = (await repository.ListarAsync(parceiroId)).ToList();
        var loginExiste = await repository.ExisteLoginAsync(entity.Login!, parceiroId);

        Assert.True(id > 0);
        Assert.Single(usuarios);
        Assert.True(loginExiste);
    }

    [Fact]
    public async Task ValidarRegistroAsync_DeveRetornarFlagsCorretos()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Operador') RETURNING id");
        await _fixture.ExecuteAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId)", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new UsuarioRepository(uow);

        var resultado = await repository.ValidarRegistroAsync("login", parceiroId, perfilId);

        Assert.True(resultado.ParceiroExiste);
        Assert.True(resultado.LoginExiste);
        Assert.True(resultado.PerfilExiste);
    }

    [Fact]
    public async Task ObterDadosLoginPorIdAsync_DeveRetornarUsuarioPerfilEEscopos()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");
        var escopoId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('manage') RETURNING id");
        await _fixture.ExecuteAsync("INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId)", new { perfilId, escopoId });
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new AuthRepository(uow);

        var login = await repo.ObterDadosLoginPorIdAsync(usuarioId, CancellationToken.None);

        Assert.NotNull(login.Usuario);
        Assert.Equal(usuarioId, login.Usuario!.Id);
        Assert.Equal("Admin", login.Perfil?.Nome);
        Assert.Contains("manage", login.Escopos);
    }

    [Fact]
    public async Task ExisteUsuarioAsync_DeveRetornarTrueSeUsuarioExistir()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Perfil') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new UsuarioRepository(uow);

        var existe = await repo.ExisteUsuarioAsync(usuarioId, parceiroId);
        Assert.True(existe);
    }

    [Fact]
    public async Task ExisteUsuarioAsync_DeveRetornarFalseSeNaoExistir()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new UsuarioRepository(uow);

        var existe = await repo.ExisteUsuarioAsync(9999, Guid.NewGuid());
        Assert.False(existe);
    }

    [Fact]
    public async Task ExisteLoginAsync_DeveRetornarFalseSeLoginNaoExistir()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new UsuarioRepository(uow);

        var existe = await repo.ExisteLoginAsync("naoexiste", Guid.NewGuid());
        Assert.False(existe);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarVazioSeNaoHouverUsuarios()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new UsuarioRepository(uow);

        var usuarios = (await repo.ListarAsync(parceiroId)).ToList();
        Assert.Empty(usuarios);
    }

    [Fact]
    public async Task ValidarRegistroAsync_DeveRetornarFalseParaTodosSeNaoHouverDados()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new UsuarioRepository(uow);

        var result = await repo.ValidarRegistroAsync("naoexiste", Guid.NewGuid(), 9999);
        Assert.False(result.ParceiroExiste);
        Assert.False(result.Data.LoginExiste);
        Assert.False(result.PerfilExiste);
    }
}
