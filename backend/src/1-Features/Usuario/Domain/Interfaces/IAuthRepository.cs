using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<UsuarioCommand?> ObterPorNomeEParceiroAsync(string login, string parceiroId, CancellationToken cancellationToken = default);
        Task<UsuarioPerfilItemQuery> ObterPerfisDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<string>> ObterEscoposDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<LoginDataQuery> ObterDadosLoginAsync(string login, CancellationToken cancellationToken = default);
        Task<LoginDataQuery> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<int> InserirRecuperacaoSenhaAsync(RecuperacaoSenhaCommand entity, CancellationToken cancellationToken = default);
        Task<RecuperacaoSenhaQuery?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default);
        Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash, CancellationToken cancellationToken = default);
    }
}
