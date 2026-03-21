using Portal.Dominio.Entities;
using Portal.Features.Auth.Infra;

namespace sso.repositories;

[Collection("database")]
public sealed class TokenAtualizacaoRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public TokenAtualizacaoRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InserirEObterPorTokenAsync_DevePersistirETrazerToken()
    {
        await _fixture.ResetAsync();
        // Cria dependência: usuario
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('User') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new TokenAtualizacaoRepository(uow);
        var token = new TokenAtualizacaoEntity
        {
            Id = 1,
            Token = "tok123",
            ExpiraEm = DateTime.UtcNow.AddDays(1),
            Revogado = false,
            UsuarioId = usuarioId
        };
        await repo.InserirAsync(token);

        var tokenDb = await repo.ObterPorTokenAsync("tok123");
        Assert.NotNull(tokenDb);
        Assert.Equal("tok123", tokenDb!.Token);
        Assert.Equal(usuarioId, tokenDb.UsuarioId);
        Assert.False(tokenDb.Revogado);
    }

    [Fact]
    public async Task RevogarAsync_DeveAtualizarFlagRevogado()
    {
        await _fixture.ResetAsync();
        var parceiroId = Guid.NewGuid();
        await _fixture.ExecuteAsync("INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@id, 'Parceiro', 'desc', true)", new { id = parceiroId });
        var perfilId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.perfil (nome) VALUES ('User') RETURNING id");
        var usuarioId = await _fixture.ExecuteScalarAsync("INSERT INTO sso.usuario (nome, email, login, senha, parceiro_id, perfil_id) VALUES ('User', 'user@t.com', 'login', 'hash', @parceiroId, @perfilId) RETURNING id", new { parceiroId, perfilId });
        await _fixture.ExecuteAsync("INSERT INTO sso.token_atualizacao (id, token, expira_em, revogado, usuario_id) VALUES (@id, @token, @expira, false, @usuarioId)",
            new { id = 1, token = "tokrev", expira = DateTime.UtcNow.AddDays(1), usuarioId });

        using var uow = _fixture.CreateUnitOfWork();
        var repo = new TokenAtualizacaoRepository(uow);
        await repo.RevogarAsync("tokrev");

        var tokenDb = await repo.ObterPorTokenAsync("tokrev");
        Assert.NotNull(tokenDb);
        Assert.True(tokenDb!.Revogado);
    }
}
