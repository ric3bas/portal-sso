using Portal.Features.Usuario.Infra;


namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface ITokenAtualizacaoRepository
    {
        Task<TokenAtualizacaoQuery?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
        Task InserirAsync(TokenAtualizacaoCommand token, CancellationToken cancellationToken = default);
        Task RevogarAsync(string token, CancellationToken cancellationToken = default);
    }
}
