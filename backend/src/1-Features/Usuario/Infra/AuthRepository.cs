using Portal.Infra;
using Dapper;
using Portal.Features.Usuario.Domain.Interfaces;

namespace Portal.Features.Usuario.Infra
{
    [DbContext("SSO_POSTGRES")]
    public class AuthRepository : DapperRepository, IDapperRepository, IAuthRepository
    {
        public AuthRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}

        public async Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE sso.usuario SET senha = @novaSenha WHERE id = @usuarioId";
            var result = await ExecuteAsync(sql, new { novaSenha = novaSenhaHash, usuarioId });
            return result > 0;
        }

        public async Task<UsuarioCommand?> ObterPorNomeEParceiroAsync(string login, string parceiroId, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT u.id,
                                        u.nome,
                                        u.login,
                                        u.senha,
                                        u.email,
                                        u.parceiro_id AS ParceiroId
                                 FROM sso.usuario u
                                 WHERE u.login = @login
                                   AND u.parceiro_id::text = @parceiroId
                                 LIMIT 1";
            return await QuerySingleAsync<UsuarioCommand>(sql, new { login, parceiroId });
        }

        public async Task<UsuarioPerfilItemQuery> ObterPerfisDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT p.id,
                                p.nome
                         FROM sso.usuario u
                         INNER JOIN sso.perfil p ON p.id = u.perfil_id
                         WHERE u.id = @usuarioId";;

            return await QuerySingleAsync<UsuarioPerfilItemQuery>(sql, new { usuarioId }) ?? new UsuarioPerfilItemQuery();
        }

        public async Task<IReadOnlyCollection<string>> ObterEscoposDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT DISTINCT e.nome
                                 FROM sso.usuario u
                                 INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
                                 INNER JOIN sso.escopo e ON e.id = pe.escopo_id
                                 WHERE u.id = @usuarioId
                                 ORDER BY e.nome";
            var escopos = await QueryAsync<string>(sql, new { usuarioId });
            return escopos.ToList();
        }

        public async Task<LoginDataQuery> ObterDadosLoginAsync(string login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT u.id, u.nome, u.login, u.senha, u.email, u.parceiro_id AS ParceiroId, 
                u.tentativas_login TentativasLogin, u.bloqueado Bloqueado, u.ativo Ativo
                FROM sso.usuario u
                WHERE u.login = @login or u.email = @login
                LIMIT 1;

                SELECT p.id, p.nome, p.is_master As IsMaster
                FROM sso.usuario u
                INNER JOIN sso.perfil p ON p.id = u.perfil_id
                WHERE u.login = @login or u.email = @login
                LIMIT 1;

                SELECT DISTINCT e.nome
                FROM sso.usuario u
                INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
                INNER JOIN sso.escopo e ON e.id = pe.escopo_id
                WHERE u.login = @login or u.email = @login
                ORDER BY e.nome;";

            using var multi = await QueryMultipleAsync(sql, new { login });

            return new LoginDataQuery
            {
                Usuario = await multi.ReadSingleOrDefaultAsync<UsuarioQuery>(),
                Perfil  = await multi.ReadSingleOrDefaultAsync<UsuarioPerfilItemQuery>(),
                Escopos = (await multi.ReadAsync<string>()).ToList()
            };
        }

        public async Task<LoginDataQuery> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT u.id, u.nome, u.login, u.senha, u.email, u.parceiro_id AS ParceiroId
                FROM sso.usuario u
                WHERE u.id = @usuarioId
                LIMIT 1;

                SELECT p.id, p.nome
                FROM sso.usuario u
                INNER JOIN sso.perfil p ON p.id = u.perfil_id
                WHERE u.id = @usuarioId
                LIMIT 1;

                SELECT DISTINCT e.nome
                FROM sso.usuario u
                INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
                INNER JOIN sso.escopo e ON e.id = pe.escopo_id
                WHERE u.id = @usuarioId
                ORDER BY e.nome;";

            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { usuarioId }, _unitOfWork.Transaction);

            return new LoginDataQuery
            {
                Usuario = await multi.ReadSingleOrDefaultAsync<UsuarioQuery>(),
                Perfil  = await multi.ReadSingleOrDefaultAsync<UsuarioPerfilItemQuery>(),
                Escopos = (await multi.ReadAsync<string>()).ToList()
            };
        }
        // Recuperação de Senha
        public async Task<int> InserirRecuperacaoSenhaAsync(RecuperacaoSenhaCommand entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"INSERT INTO sso.recuperacao_senha (usuario_id, token, expira_em, usado)
                                 VALUES (@UsuarioId, @Token, @ExpiraEm, @Usado)
                                 RETURNING id";
            return await Task.Run(() => QuerySingle<int>(sql, entity), cancellationToken);
        }

        public async Task<RecuperacaoSenhaQuery?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"SELECT id, usuario_id AS UsuarioId, token, expira_em AS ExpiraEm, usado
                                 FROM sso.recuperacao_senha
                                 WHERE token = @token
                                 AND usado = FALSE
                                 ORDER BY id DESC
                                 LIMIT 1";
            return await Task.Run(() => QuerySingleAsync<RecuperacaoSenhaQuery>(sql, new { token }), cancellationToken);
        }

        public async Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.recuperacao_senha SET usado = TRUE WHERE id = @id";
            await Task.Run(() => Execute(sql, new { id }), cancellationToken);
        }
    }
}
