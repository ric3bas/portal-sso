using Dapper;
using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class AuthRepository : DataDapperRepository, IAuthRepository
{
    public AuthRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.usuario SET senha = @novaSenha WHERE id = @usuarioId";
        var result = await ExecuteAsync(sql, new { novaSenha = novaSenhaHash, usuarioId });
        return result > 0;
    }

    public async Task<LoginDataQuery> ObterDadosLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        const string usuarioSql = @"
            SELECT u.id, u.nome, u.login, u.senha, u.email, u.parceiro_id AS ParceiroId,
                   u.tentativas_login as TentativasLogin, u.bloqueado as Bloqueado, u.ativo as Ativo
            FROM sso.usuario u
            WHERE u.login = @login or u.email = @login
            LIMIT 1;";

        const string perfilSql = @"
            SELECT p.id, p.nome, p.is_master As IsMaster
            FROM sso.usuario u
            INNER JOIN sso.perfil p ON p.id = u.perfil_id
            WHERE u.login = @login or u.email = @login
            LIMIT 1;";

        const string escoposSql = @"
            SELECT DISTINCT e.nome
            FROM sso.usuario u
            INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
            INNER JOIN sso.escopo e ON e.id = pe.escopo_id
            WHERE u.login = @login or u.email = @login
            ORDER BY e.nome;";

        var usuario = await QuerySingleAsync<UsuarioQuery>(usuarioSql, new { login });
        var perfil = await QuerySingleAsync<UsuarioPerfilQuery>(perfilSql, new { login });
        var escopos = await QueryAsync<string>(escoposSql, new { login });

        return new LoginDataQuery { Usuario = usuario, Perfil = perfil, Escopos = escopos.ToList() };
    }

    public async Task<LoginDataQuery> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string usuarioSql = @"
            SELECT u.id, u.nome, u.login, u.senha, u.email, u.parceiro_id AS ParceiroId,
                   u.tentativas_login as TentativasLogin, u.bloqueado as Bloqueado, u.ativo as Ativo
            FROM sso.usuario u
            WHERE u.id = @usuarioId
            LIMIT 1;";

        const string perfilSql = @"
            SELECT p.id, p.nome, p.is_master As IsMaster
            FROM sso.usuario u
            INNER JOIN sso.perfil p ON p.id = u.perfil_id
            WHERE u.id = @usuarioId
            LIMIT 1;";

        const string escoposSql = @"
            SELECT DISTINCT e.nome
            FROM sso.usuario u
            INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
            INNER JOIN sso.escopo e ON e.id = pe.escopo_id
            WHERE u.id = @usuarioId
            ORDER BY e.nome;";

        var usuario = await QuerySingleAsync<UsuarioQuery>(usuarioSql, new { usuarioId });
        var perfil = await QuerySingleAsync<UsuarioPerfilQuery>(perfilSql, new { usuarioId });
        var escopos = await QueryAsync<string>(escoposSql, new { usuarioId });

        return new LoginDataQuery { Usuario = usuario, Perfil = perfil, Escopos = escopos.ToList() };
    }

    public async Task<int> InserirRecuperacaoSenhaAsync(int usuarioId, string token, DateTime expiraEm, bool usado, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO sso.recuperacao_senha (usuario_id, token, expira_em, usado)
                             VALUES (@UsuarioId, @Token, @ExpiraEm, @Usado)
                             RETURNING id";
        var result = await QuerySingleAsync<int>(sql, new { UsuarioId = usuarioId, Token = token, ExpiraEm = expiraEm, Usado = usado });
        return result;
    }

    public async Task<RecuperacaoSenhaQuery?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id, usuario_id AS UsuarioId, token, expira_em AS ExpiraEm, usado
                             FROM sso.recuperacao_senha
                             WHERE token = @token
                               AND usado = FALSE
                             ORDER BY id DESC
                             LIMIT 1";
        return await QuerySingleAsync<RecuperacaoSenhaQuery>(sql, new { token });
    }

    public async Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.recuperacao_senha SET usado = TRUE WHERE id = @id";
        await ExecuteAsync(sql, new { id });
    }
}
