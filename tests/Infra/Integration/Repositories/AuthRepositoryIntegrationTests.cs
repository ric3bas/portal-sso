using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Infra;
using Npgsql;
using Portal.Dominio.Entities;
using Portal.Features.Auth.Infra;
using Xunit;

namespace sso.repositories;

[Collection("database")]
public sealed class AuthRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public AuthRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ObterDadosLoginAsync_DeveRetornarUsuarioPerfilEEscopos()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");
        var escopoId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.escopo (nome) VALUES ('manage') RETURNING id");
        await _fixture.ExecuteAsync("INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId)", new { perfilId, escopoId });
        const int usuarioId = 4;
        await _fixture.ExecuteAsync("INSERT INTO sso.usuario (id, nome, email, login, senha, parceiro_id, perfil_id) VALUES (@usuarioId, 'User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId)", new { usuarioId, parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var login = await repository.ObterDadosLoginAsync("login", CancellationToken.None);
        var escopos = await repository.ObterEscoposDoUsuarioAsync(usuarioId, CancellationToken.None);

        Assert.Equal(usuarioId, login.Usuario?.Id);
        Assert.Equal("Admin", login.Perfil?.Nome);
        Assert.Contains("manage", login.Escopos);
        Assert.Contains("manage", escopos);
    }

    [Fact]
    public async Task ObterPorNomeEParceiroAsync_DeveRetornarUsuario()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new AuthRepository(uow);

        var usuario = await repo.ObterPorNomeEParceiroAsync("login", parceiroId.ToString(), CancellationToken.None);
        Assert.NotNull(usuario);
        Assert.Equal(usuarioId, usuario!.Id);
    }

    [Fact]
    public async Task ObterPorNomeEParceiroAsync_DeveRetornarNullSeNaoExistir()
    {
        await _fixture.ResetAsync();
        using var uow = _fixture.CreateUnitOfWork();
        var repo = new AuthRepository(uow);

        var usuario = await repo.ObterPorNomeEParceiroAsync("naoexiste", Guid.NewGuid().ToString(), CancellationToken.None);
        Assert.Null(usuario);
    }

    [Fact]
    public async Task ObterPerfisDoUsuarioAsync_DeveRetornarPerfil()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new AuthRepository(uow);

        var perfil = await repo.ObterPerfisDoUsuarioAsync(usuarioId, CancellationToken.None);
        Assert.NotNull(perfil);
        Assert.Equal(perfilId, perfil.Id);
    }

    [Fact]
    public async Task ObterEscoposDoUsuarioAsync_DeveRetornarVazioSeNaoTiverEscopos()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Admin') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new AuthRepository(uow);

        var escopos = await repo.ObterEscoposDoUsuarioAsync(usuarioId, CancellationToken.None);
        Assert.NotNull(escopos);
        Assert.Empty(escopos);
    }

    [Fact]
    public async Task InserirRecuperacaoSenhaAsync_DeveRetornarIdERegistrar()
    {
        await _fixture.ResetAsync();
        var usuarioId = await CriarUsuarioAsync("recuperacao");
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var entidade = new RecuperacaoSenhaEntity
        {
            UsuarioId = usuarioId,
            Token = "token-insert",
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            Usado = false
        };

        var id = await repository.InserirRecuperacaoSenhaAsync(entidade);

        Assert.True(id > 0);
        var registrado = await repository.ObterRecuperacaoSenhaPorTokenAsync(entidade.Token);
        Assert.NotNull(registrado);
        Assert.Equal(usuarioId, registrado!.UsuarioId);
        Assert.False(registrado.Usado);
    }

    [Fact]
    public async Task ObterRecuperacaoSenhaPorTokenAsync_DeveRetornarRegistroMaisRecenteNaoUsado()
    {
        await _fixture.ResetAsync();
        var usuarioId = await CriarUsuarioAsync("recuperacao2");
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var primeiro = new RecuperacaoSenhaEntity
        {
            UsuarioId = usuarioId,
            Token = "dup-token",
            ExpiraEm = DateTime.UtcNow.AddMinutes(10),
            Usado = false
        };
        var segundo = new RecuperacaoSenhaEntity
        {
            UsuarioId = usuarioId,
            Token = "dup-token",
            ExpiraEm = DateTime.UtcNow.AddMinutes(20),
            Usado = false
        };

        var primeiroId = await repository.InserirRecuperacaoSenhaAsync(primeiro);
        var segundoId = await repository.InserirRecuperacaoSenhaAsync(segundo);

        var resultado = await repository.ObterRecuperacaoSenhaPorTokenAsync("dup-token");

        Assert.NotNull(resultado);
        Assert.Equal(segundoId, resultado!.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
    }

    [Fact]
    public async Task MarcarRecuperacaoSenhaComoUsadoAsync_DeveAtualizarFlagUsado()
    {
        await _fixture.ResetAsync();
        var usuarioId = await CriarUsuarioAsync("recuperacao3");
        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var entidade = new RecuperacaoSenhaEntity
        {
            UsuarioId = usuarioId,
            Token = "token-usado",
            ExpiraEm = DateTime.UtcNow.AddMinutes(15),
            Usado = false
        };

        var id = await repository.InserirRecuperacaoSenhaAsync(entidade);

        await repository.MarcarRecuperacaoSenhaComoUsadoAsync(id);

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        var usado = await connection.QuerySingleAsync<bool>("SELECT usado FROM sso.recuperacao_senha WHERE id = @id", new { id });

        Assert.True(usado);
    }

    private async Task<int> CriarUsuarioAsync(string loginSuffix)
    {
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('Perfil_" + loginSuffix + "') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', @email, @login, 'hash', @parceiroId, @perfilId) RETURNING id",
            new { email = $"user-{loginSuffix}@teste.com", login = loginSuffix, parceiroId, perfilId });
        return usuarioId;
    }

    [Fact]
    public async Task AtualizarSenhaUsuarioAsync_DeveAtualizarSenhaEPreencherHash()
    {
        await _fixture.ResetAsync();
        var usuarioId = await CriarUsuarioAsync("senha-validar");

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var novaSenhaHash = "new-hash-value";
        var resultado = await repository.AtualizarSenhaUsuarioAsync(usuarioId, novaSenhaHash, CancellationToken.None);

        Assert.True(resultado);

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        var senhaArmazenada = await connection.QuerySingleAsync<string>("SELECT senha FROM sso.usuario WHERE id = @usuarioId", new { usuarioId });

        Assert.Equal(novaSenhaHash, senhaArmazenada);
    }

    [Fact]
    public async Task AtualizarSenhaUsuarioAsync_UsuarioInexistente_RetornaFalse()
    {
        await _fixture.ResetAsync();

        using var uow = _fixture.CreateUnitOfWork();
        var repository = new AuthRepository(uow);

        var resultado = await repository.AtualizarSenhaUsuarioAsync(9999, "hash", CancellationToken.None);

        Assert.False(resultado);
    }
}
