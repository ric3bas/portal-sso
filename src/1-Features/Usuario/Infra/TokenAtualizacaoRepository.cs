using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Infra;


namespace Portal.Features.Usuario.Infra
{
    [DbContext("SSO_POSTGRES")]
    public class TokenAtualizacaoRepository : DapperRepository, IDapperRepository, ITokenAtualizacaoRepository
    {
        public TokenAtualizacaoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<TokenAtualizacaoQuery?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"SELECT id,
                                        token,
                                        expira_em AS ExpiraEm,
                                        revogado,
                                        usuario_id AS UsuarioId
                                 FROM sso.token_atualizacao
                                 WHERE token = @token
                                 ORDER BY id DESC
                                 LIMIT 1";
            return await QuerySingleAsync<TokenAtualizacaoQuery>(sql, new { token });
        }

        public async Task InserirAsync(TokenAtualizacaoCommand token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            const string sql = @"INSERT INTO sso.token_atualizacao (token, expira_em, revogado, usuario_id)
                                 VALUES (@Token, @ExpiraEm, @Revogado, @UsuarioId)";
            await Task.Run(() => Execute(sql, token));
        }

        public async Task RevogarAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "UPDATE sso.token_atualizacao SET revogado = TRUE WHERE token = @token";
            await Task.Run(() => Execute(sql, new { token }));
        }
    }
}
