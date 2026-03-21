using Portal.Infra;
using Portal.Features.Auth.Domain.Interfaces;
using Dapper;
using Portal.Features.Auth.Domain.Responses;
using Portal.Features.Auth.Domain;

namespace Portal.Features.Auth.Infra
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

        public async Task<Portal.Dominio.Entities.UsuarioEntity?> ObterPorNomeEParceiroAsync(string login, string parceiroId, CancellationToken cancellationToken)
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
            return await QuerySingleAsync<Portal.Dominio.Entities.UsuarioEntity>(sql, new { login, parceiroId });
        }

        public async Task<UsuarioPerfilItemResponse?> ObterPerfisDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT p.id,
                                p.nome
                         FROM sso.usuario u
                         INNER JOIN sso.perfil p ON p.id = u.perfil_id
                         WHERE u.id = @usuarioId";;

            return await QuerySingleAsync<UsuarioPerfilItemResponse>(sql, new { usuarioId });
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

        public async Task<LoginData> ObterDadosLoginAsync(string login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT u.id, u.nome, u.login, u.senha, u.email, u.parceiro_id AS ParceiroId
                FROM sso.usuario u
                WHERE u.login = @login
                LIMIT 1;

                SELECT p.id, p.nome
                FROM sso.usuario u
                INNER JOIN sso.perfil p ON p.id = u.perfil_id
                WHERE u.login = @login
                LIMIT 1;

                SELECT DISTINCT e.nome
                FROM sso.usuario u
                INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = u.perfil_id
                INNER JOIN sso.escopo e ON e.id = pe.escopo_id
                WHERE u.login = @login
                ORDER BY e.nome;";

            using var multi = await QueryMultipleAsync(sql, new { login });

            return new LoginData
            {
                Usuario = await multi.ReadSingleOrDefaultAsync<Dominio.Entities.UsuarioEntity>(),
                Perfil  = await multi.ReadSingleOrDefaultAsync<UsuarioPerfilItemResponse>(),
                Escopos = (await multi.ReadAsync<string>()).ToList()
            };
        }

        public async Task<LoginData> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken)
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

            return new LoginData
            {
                Usuario = await multi.ReadSingleOrDefaultAsync<Portal.Dominio.Entities.UsuarioEntity>(),
                Perfil  = await multi.ReadSingleOrDefaultAsync<UsuarioPerfilItemResponse>(),
                Escopos = (await multi.ReadAsync<string>()).ToList()
            };
        }
        // Recuperação de Senha
        public async Task<int> InserirRecuperacaoSenhaAsync(Portal.Dominio.Entities.RecuperacaoSenhaEntity entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"INSERT INTO sso.recuperacao_senha (usuario_id, token, expira_em, usado)
                                 VALUES (@UsuarioId, @Token, @ExpiraEm, @Usado)
                                 RETURNING id";
            return await Task.Run(() => QuerySingle<int>(sql, entity), cancellationToken);
        }

        public async Task<Portal.Dominio.Entities.RecuperacaoSenhaEntity?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"SELECT id, usuario_id AS UsuarioId, token, expira_em AS ExpiraEm, usado
                                 FROM sso.recuperacao_senha
                                 WHERE token = @token
                                 AND usado = FALSE
                                 ORDER BY id DESC
                                 LIMIT 1";
            return await Task.Run(() => QuerySingleAsync<Dominio.Entities.RecuperacaoSenhaEntity>(sql, new { token }), cancellationToken);
        }

        public async Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.recuperacao_senha SET usado = TRUE WHERE id = @id";
            await Task.Run(() => Execute(sql, new { id }), cancellationToken);
        }
    }
}
