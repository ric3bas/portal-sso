using Portal.Dominio.Entities;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Auth.Infra
{
    public class TokenAtualizacaoRepository : DapperRepository<TokenAtualizacao>, IDapperRepository<TokenAtualizacao>, ITokenAtualizacaoRepository
    {
        public TokenAtualizacaoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<TokenAtualizacao?> ObterPorTokenAsync(string token)
        {
            const string sql = @"SELECT id,
                                        token,
                                        expira_em AS ExpiraEm,
                                        revogado,
                                        usuario_id AS UsuarioId
                                 FROM sso.token_atualizacao
                                 WHERE token = @token
                                 ORDER BY id DESC
                                 LIMIT 1";
            return await Task.Run(() => QuerySingle(sql, new { token }));
        }

        public async Task InserirAsync(TokenAtualizacao token)
        {
            const string sql = @"INSERT INTO sso.token_atualizacao (token, expira_em, revogado, usuario_id)
                                 VALUES (@Token, @ExpiraEm, @Revogado, @UsuarioId)";
            await Task.Run(() => Execute(sql, token));
        }

        public async Task RevogarAsync(string token)
        {
            const string sql = "UPDATE sso.token_atualizacao SET revogado = TRUE WHERE token = @token";
            await Task.Run(() => Execute(sql, new { token }));
        }
    }
}
