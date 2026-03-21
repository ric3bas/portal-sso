using Portal.Dominio.Entities;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface ITokenAtualizacaoRepository
    {
        Task<TokenAtualizacaoEntity?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
        Task InserirAsync(TokenAtualizacaoEntity token, CancellationToken cancellationToken = default);
        Task RevogarAsync(string token, CancellationToken cancellationToken = default);
    }
}
