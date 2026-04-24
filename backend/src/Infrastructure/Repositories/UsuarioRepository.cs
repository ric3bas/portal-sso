using Dapper;
using Portal.Domain.Common;
using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class UsuarioRepository : DataDapperRepository, IUsuarioRepository
{
    private const string PerfilMaster = "MASTER";

    public UsuarioRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<int> InserirAsync(UsuarioCommand usuario, CancellationToken cancellationToken)
    {
        const string sql = "INSERT INTO sso.usuario (nome, email,login, senha, parceiro_id, perfil_id, tentativas_login, bloqueado, ativo) " +
                           "VALUES (@Nome, @Email, @Login, @Senha, @ParceiroId, @PerfilId, @TentativasLogin, @Bloqueado, @Ativo) RETURNING id";

        var result = await QueryAsync<int>(sql, new
        {
            usuario.Nome,
            usuario.Email,
            usuario.Login,
            usuario.Senha,
            usuario.ParceiroId,
            usuario.PerfilId,
            usuario.TentativasLogin,
            usuario.Bloqueado,
            usuario.Ativo
        });

        return result.FirstOrDefault();
    }

    public async Task<(bool ParceiroExiste, bool LoginExiste, bool PerfilExiste)> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default)
    {
        const string parceiroSql = "SELECT COUNT(1) FROM sso.parceiro WHERE id = @ParceiroId";
        const string loginSql = "SELECT COUNT(1) FROM sso.usuario WHERE login = @Login AND parceiro_id = @ParceiroId";
        const string perfilSql = "SELECT COUNT(1) FROM sso.perfil WHERE id = @PerfilId and upper(nome) != @Master";

        var parceiroExiste = (await QuerySingleAsync<int>(parceiroSql, new { ParceiroId = parceiroId })) > 0;
        var loginExiste = (await QuerySingleAsync<int>(loginSql, new { Login = login, ParceiroId = parceiroId })) > 0;
        var perfilExiste = (await QuerySingleAsync<int>(perfilSql, new { PerfilId = perfilId, Master = PerfilMaster })) > 0;

        return (parceiroExiste, loginExiste, perfilExiste);
    }

    public async Task<ResultadoPaginado<UsuarioComPerfilQuery>> ObterPorParceiroAsync(Guid? parceiroId, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        const string sql = @"SELECT u.id,
                                    u.nome,
                                    u.login,
                                    u.email,
                                    p.nome AS Perfil,
                                    pp.nome AS Parceiro,
                                    u.ativo AS Ativo,
                                    u.bloqueado AS Bloqueado
                             FROM sso.usuario u
                             INNER JOIN sso.perfil p ON p.id = u.perfil_id
                             INNER JOIN sso.parceiro pp ON pp.id = u.parceiro_id
                             {WHERE}
                             ORDER BY u.nome {ORDER}
                             LIMIT @TamanhoPagina OFFSET @Offset";

        const string sqlCount = @"SELECT COUNT(1)
                                  FROM sso.usuario u
                                  {WHERE}";

        var temFiltro = parceiroId.HasValue && parceiroId != Guid.Empty;
        var query = sql.Replace("{WHERE}", temFiltro ? "WHERE u.parceiro_id = @parceiroId" : string.Empty)
                       .Replace("{ORDER}", paginacao.OrdemSql);
        var queryCount = sqlCount.Replace("{WHERE}", temFiltro ? "WHERE u.parceiro_id = @parceiroId" : string.Empty);

        var total = await QuerySingleAsync<int>(queryCount, new { parceiroId });
        var itens = (await QueryAsync<UsuarioComPerfilQuery>(query, new { parceiroId, TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset })).ToList();
        return new ResultadoPaginado<UsuarioComPerfilQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task IncrementarTentativaLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.usuario SET tentativas_login = tentativas_login + 1 WHERE id = @usuarioId";
        await ExecuteAsync(sql, new { usuarioId });
    }

    public async Task ResetarTentativasLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.usuario SET tentativas_login = 0 WHERE id = @usuarioId";
        await ExecuteAsync(sql, new { usuarioId });
    }

    public async Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.usuario SET bloqueado = true WHERE id = @usuarioId";
        await ExecuteAsync(sql, new { usuarioId });
    }

    public async Task<UsuarioQuery?> ObterPorIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id, nome, email, login, senha,
                                    parceiro_id as ParceiroId,
                                    perfil_id as PerfilId,
                                    tentativas_login as TentativasLogin,
                                    bloqueado, ativo
                             FROM sso.usuario
                             WHERE id = @usuarioId";
        return await QuerySingleAsync<UsuarioQuery>(sql, new { usuarioId });
    }

    public async Task AtualizarAsync(UsuarioCommand usuario, CancellationToken cancellationToken = default)
    {
        const string sql = @"UPDATE sso.usuario
                             SET nome = @Nome, login = @Login, email = @Email, ativo = @Ativo, bloqueado = @Bloqueado
                             WHERE id = @Id";
        await ExecuteAsync(sql, new { usuario.Id, usuario.Nome, usuario.Login, usuario.Email, usuario.Ativo, usuario.Bloqueado });
    }
}

