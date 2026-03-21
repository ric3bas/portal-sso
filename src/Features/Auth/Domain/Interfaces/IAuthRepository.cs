using Portal.Dominio.Entities;
using Portal.Features.Auth.Domain.Responses;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface IAuthRepository
    {
        // Task<IEnumerable<UsuarioComPerfilResponse>> ListarComPerfisAsync(CancellationToken cancellationToken = default);
        Task<Portal.Dominio.Entities.UsuarioEntity?> ObterPorNomeEParceiroAsync(string login, string parceiroId, CancellationToken cancellationToken = default);
        Task<UsuarioPerfilItemResponse> ObterPerfisDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<string>> ObterEscoposDoUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<LoginData> ObterDadosLoginAsync(string login, CancellationToken cancellationToken = default);
        Task<LoginData> ObterDadosLoginPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
        // Task<Portal.Dominio.Entities.Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
        // Task<Portal.Dominio.Entities.Usuario?> ObterPorIdAsync(int id);
        // Task<IEnumerable<Portal.Dominio.Entities.Usuario>?> ListarAsync();
        // Task<int> InserirAsync(Portal.Dominio.Entities.Usuario usuario);
        // Task VincularPerfisAsync(int usuarioId, IReadOnlyCollection<int> perfilIds);
        // Task<bool> PerfisPertencemAoParceiroAsync(Guid parceiroId, IReadOnlyCollection<int> perfilIds);
        // Recuperação de Senha
        Task<int> InserirRecuperacaoSenhaAsync(RecuperacaoSenhaEntity entity, CancellationToken cancellationToken = default);
        Task<RecuperacaoSenhaEntity?> ObterRecuperacaoSenhaPorTokenAsync(string token, CancellationToken cancellationToken = default);
        Task MarcarRecuperacaoSenhaComoUsadoAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash, CancellationToken cancellationToken = default);
    }
}
