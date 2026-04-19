using Portal.Domain.Usuario;

namespace Portal.Domain.Usuario.Interfaces;

public interface ITokenAtualizacaoRepository
{
    Task<TokenAtualizacaoQuery?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
    Task InserirAsync(TokenAtualizacaoCommand token, CancellationToken cancellationToken = default);
    Task RevogarAsync(string token, CancellationToken cancellationToken = default);
}
