using Dapper;
using Portal.Dominio.Entities;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Usuario.Infra
{
    public class UsuarioRepository : DapperRepository<Portal.Dominio.Entities.UsuarioEntity>, IDapperRepository<Portal.Dominio.Entities.UsuarioEntity>, IUsuarioRepository
    {
        public UsuarioRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}
        public readonly string perfilMaster = "MASTER";

        public async Task<int> InserirAsync(UsuarioEntity usuario, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "INSERT INTO sso.usuario (nome, email,login, senha, parceiro_id, perfil_id) " +
                "VALUES (@Nome, @Email, @login, @Senha, @ParceiroId, @PerfilId) RETURNING id";

            var perfilId = usuario.PerfilId == default(int) ? null : usuario.PerfilId;

            return await Task.Run(() => _unitOfWork.Connection.QuerySingle<int>(sql, new { 
                usuario.Nome, 
                usuario.Email, 
                usuario.Login, 
                usuario.Senha, 
                usuario.ParceiroId,
                perfilId}, _unitOfWork.Transaction), cancellationToken);
        }

        public async Task<bool> ExisteLoginAsync(string login, Guid parceiroId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT COUNT(1) FROM sso.usuario WHERE login = @Login AND parceiro_id = @ParceiroId";
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { Login = login, ParceiroId = parceiroId }, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<RegistroValidacao> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT COUNT(1) FROM sso.parceiro WHERE id = @ParceiroId;
                SELECT COUNT(1) FROM sso.usuario  WHERE login = @Login AND parceiro_id = @ParceiroId;
                SELECT COUNT(1) FROM sso.perfil   WHERE id = @PerfilId and upper(nome) != @Master;";

            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, 
                new { 
                    Login = login, 
                    ParceiroId = parceiroId, 
                    PerfilId = perfilId,
                    Master = perfilMaster
                }, _unitOfWork.Transaction);

            return new RegistroValidacao
            {
                ParceiroExiste = await multi.ReadSingleAsync<int>() > 0,
                LoginExiste    = await multi.ReadSingleAsync<int>() > 0,
                PerfilExiste   = await multi.ReadSingleAsync<int>() > 0
            };
        }

        public async Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(Guid parceiroId, CancellationToken cancellationToken = default)
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
            return await Task.Run(() => _unitOfWork.Connection.Query<UsuarioComPerfilResponse>(sql, new { parceiroId }, _unitOfWork.Transaction), cancellationToken);
        }

        public async Task<bool> ExisteUsuarioAsync(int usuarioId, Guid parceiroId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.usuario WHERE id = @usuarioId AND parceiro_id = @parceiroId";
            var total = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { usuarioId, parceiroId }, _unitOfWork.Transaction);
            return total > 0;
        }

        public async Task IncrementarTentativaLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.usuario SET tentativas_login = tentativas_login + 1, ultimo_erro_login = NOW() WHERE id = @usuarioId";
            await _unitOfWork.Connection.ExecuteAsync(sql, new { usuarioId }, _unitOfWork.Transaction);
        }

        public async Task ResetarTentativasLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.usuario SET tentativas_login = 0, ultimo_erro_login = NULL WHERE id = @usuarioId";
            await _unitOfWork.Connection.ExecuteAsync(sql, new { usuarioId }, _unitOfWork.Transaction);
        }

        public async Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "UPDATE sso.usuario SET tentativas_login = 5 WHERE id = @usuarioId";
            await _unitOfWork.Connection.ExecuteAsync(sql, new { usuarioId }, _unitOfWork.Transaction);
        }

        public async Task<UsuarioEntity?> ObterPorIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT id, nome, email, login, senha, parceiro_id, perfil_id, tentativas_login, ultimo_erro_login FROM sso.usuario WHERE id = @usuarioId";
            return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<UsuarioEntity>(sql, new { usuarioId }, _unitOfWork.Transaction);
        }

        public async Task AtualizarAsync(UsuarioEntity usuario, CancellationToken cancellationToken = default)
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
            await _unitOfWork.Connection.ExecuteAsync(sql, usuario, _unitOfWork.Transaction);
        }

    }
}


