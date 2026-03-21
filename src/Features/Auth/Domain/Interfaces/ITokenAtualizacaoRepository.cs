using Portal.Dominio.Entities;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface ITokenAtualizacaoRepository
    {
        Task<TokenAtualizacao?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
        Task InserirAsync(TokenAtualizacao token, CancellationToken cancellationToken = default);
        Task RevogarAsync(string token, CancellationToken cancellationToken = default);
    }
}
