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
            cancellationToken.ThrowIfCancellationRequested();
            
            const string sql = @"INSERT INTO sso.token_atualizacao (token, expira_em, revogado, usuario_id, ip_usuario, logado_em)
                                 VALUES (@Token, @ExpiraEm, @Revogado, @UsuarioId, @IpUsuario, @LogadoEm )";
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
