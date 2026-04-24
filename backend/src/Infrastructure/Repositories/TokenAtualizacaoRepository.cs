using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class TokenAtualizacaoRepository : DataDapperRepository, ITokenAtualizacaoRepository
{
    public TokenAtualizacaoRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<TokenAtualizacaoQuery?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id,
                                    token,
                                    expira_em AS ExpiraEm,
                                    revogado,
                                    usuario_id AS UsuarioId,
                                    ip_usuario as IpUsuario
                             FROM sso.token_atualizacao
                             WHERE token = @token
                             ORDER BY id DESC
                             LIMIT 1";
        return await QuerySingleAsync<TokenAtualizacaoQuery>(sql, new { token });
    }

    public async Task InserirAsync(TokenAtualizacaoCommand token, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO sso.token_atualizacao (token, expira_em, revogado, usuario_id, ip_usuario, logado_em)
                             VALUES (@Token, @ExpiraEm, @Revogado, @UsuarioId, @IpUsuario, @LogadoEm )";
        await ExecuteAsync(sql, token);
    }

    public async Task RevogarAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.token_atualizacao SET revogado = TRUE WHERE token = @token";
        await ExecuteAsync(sql, new { token });
    }
}
