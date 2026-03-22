using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Usuario.Infra
{
    [DbContext("SSO_POSTGRES")]
    public class UsuarioRepository : DapperRepository, IDapperRepository, IUsuarioRepository
    {
        public UsuarioRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}
        public readonly string perfilMaster = "MASTER";

        public async Task<int> InserirAsync(UsuarioCommand usuario, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "INSERT INTO sso.usuario (nome, email,login, senha, parceiro_id, perfil_id) " +
                "VALUES (@Nome, @Email, @login, @Senha, @ParceiroId, @PerfilId) RETURNING id";

            var perfilId = usuario.PerfilId == default(int) ? null : usuario.PerfilId;

            var result = await QueryAsync<int>(sql, new { 
                usuario.Nome, 
                usuario.Email, 
                usuario.Login, 
                usuario.Senha, 
                usuario.ParceiroId,
                perfilId });
            return result.FirstOrDefault();
        }

        public async Task<bool> ExisteLoginAsync(string login, Guid parceiroId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT COUNT(1) FROM sso.usuario WHERE login = @Login AND parceiro_id = @ParceiroId";
            var count = await QuerySingleAsync<int>(sql, new { Login = login, ParceiroId = parceiroId });
            return count > 0;
        }

        public async Task<RegistroValidacaoQuery> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT COUNT(1) FROM sso.parceiro WHERE id = @ParceiroId;
                SELECT COUNT(1) FROM sso.usuario  WHERE login = @Login AND parceiro_id = @ParceiroId;
                SELECT COUNT(1) FROM sso.perfil   WHERE id = @PerfilId and upper(nome) != @Master;";

            using var multi = await QueryMultipleAsync(sql,
                new { 
                    Login = login, 
                    ParceiroId = parceiroId, 
                    PerfilId = perfilId,
                    Master = perfilMaster
                });

            return new RegistroValidacaoQuery
            {
                ParceiroExiste = await multi.ReadSingleAsync<int>() > 0,
                LoginExiste    = await multi.ReadSingleAsync<int>() > 0,
                PerfilExiste   = await multi.ReadSingleAsync<int>() > 0
            };
        }

        public async Task<IEnumerable<UsuarioComPerfilQuery>> ListarAsync(Guid parceiroId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"SELECT u.id,
                                        u.nome,
                                        u.login,
                                        u.email,
                                        u.parceiro_id AS ParceiroId
                                 FROM sso.usuario u
                                 WHERE u.parceiro_id = @parceiroId
                                 ORDER BY u.nome";
            return await QueryAsync<UsuarioComPerfilQuery>(sql, new { parceiroId });
        }

        public async Task<bool> ExisteUsuarioAsync(int usuarioId, Guid parceiroId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.usuario WHERE id = @usuarioId AND parceiro_id = @parceiroId";
            var total = await QuerySingleAsync<int>(sql, new { usuarioId, parceiroId });
            return total > 0;
        }

        public async Task IncrementarTentativaLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.usuario SET tentativas_login = tentativas_login + 1, ultimo_erro_login = NOW() WHERE id = @usuarioId";
            await ExecuteAsync(sql, new { usuarioId });
        }

        public async Task ResetarTentativasLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.usuario SET tentativas_login = 0, ultimo_erro_login = NULL WHERE id = @usuarioId";
            await ExecuteAsync(sql, new { usuarioId });
        }

        public async Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "UPDATE sso.usuario SET bloqueado = true WHERE id = @usuarioId";
            await ExecuteAsync(sql, new { usuarioId });
        }

        public async Task<UsuarioQuery?> ObterPorIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT id, nome, email, login, senha, parceiro_id, perfil_id, tentativas_login, ultimo_erro_login FROM sso.usuario WHERE id = @usuarioId";
            return await QuerySingleAsync<UsuarioQuery>(sql, new { usuarioId });
        }

        public async Task AtualizarAsync(UsuarioCommand usuario, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"UPDATE sso.usuario SET
                nome = @Nome,
                email = @Email,
                login = @Login,
                senha = @Senha,
                parceiro_id = @ParceiroId,
                perfil_id = @PerfilId,
                tentativas_login = @TentativasLogin,
                ultimo_erro_login = @UltimoErroLogin
                WHERE id = @Id";
            await ExecuteAsync(sql, usuario);
        }

    }
}


