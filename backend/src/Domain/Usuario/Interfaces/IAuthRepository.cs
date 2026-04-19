using Portal.Domain.Usuario;

namespace Portal.Domain.Usuario.Interfaces;

public interface IAuthRepository
{
    Task<LoginDataQuery> ObterDadosLoginAsync(string login, CancellationToken cancellationToken = default);
    Task<LoginDataQuery> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<int> InserirRecuperacaoSenhaAsync(int usuarioId, string token, DateTime expiraEm, bool usado, CancellationToken cancellationToken = default);
    Task<RecuperacaoSenhaQuery?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default);
    Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash, CancellationToken cancellationToken = default);
}
