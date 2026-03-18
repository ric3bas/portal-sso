using Portal.Dominio.Entities;
using Portal.Features.Auth.Domain.Responses;

namespace Portal.Features.Auth.Domain.Interfaces
{
    public interface IAuthRepository
    {
        // Task<IEnumerable<UsuarioComPerfilResponse>> ListarComPerfisAsync(CancellationToken cancellationToken = default);
        Task<Portal.Dominio.Entities.UsuarioEntity?> ObterPorNomeEParceiroAsync(string login, string parceiroId);
        Task<UsuarioPerfilItemResponse> ObterPerfisDoUsuarioAsync(int usuarioId);
        Task<IReadOnlyCollection<string>> ObterEscoposDoUsuarioAsync(int usuarioId);
        Task<LoginData> ObterDadosLoginAsync(string login);
        Task<LoginData> ObterDadosLoginPorIdAsync(int usuarioId);
        // Task<Portal.Dominio.Entities.Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
        // Task<Portal.Dominio.Entities.Usuario?> ObterPorIdAsync(int id);
        // Task<IEnumerable<Portal.Dominio.Entities.Usuario>?> ListarAsync();
        // Task<int> InserirAsync(Portal.Dominio.Entities.Usuario usuario);
        // Task VincularPerfisAsync(int usuarioId, IReadOnlyCollection<int> perfilIds);
        // Task<bool> PerfisPertencemAoParceiroAsync(Guid parceiroId, IReadOnlyCollection<int> perfilIds);
        // Recuperação de Senha
        Task<int> InserirRecuperacaoSenhaAsync(RecuperacaoSenhaEntity entity);
        Task<RecuperacaoSenhaEntity?> ObterRecuperacaoSenhaPorTokenAsync(string token);
        Task MarcarRecuperacaoSenhaComoUsadoAsync(int id);
        Task<bool> AtualizarSenhaUsuarioAsync(int usuarioId, string novaSenhaHash);
    }

    public interface ITokenAtualizacaoRepository
    {
        Task<TokenAtualizacao?> ObterPorTokenAsync(string token);
        Task InserirAsync(TokenAtualizacao token);
        Task RevogarAsync(string token);
    }
}
