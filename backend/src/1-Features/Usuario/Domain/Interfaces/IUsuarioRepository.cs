using Portal.Features.Usuario.Infra;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> InserirAsync(UsuarioCommand usuario, CancellationToken cancellationToken);
        Task<bool> ExisteLoginAsync(string login, Guid parceiroId, CancellationToken cancellationToken = default);
        Task<RegistroValidacaoQuery> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UsuarioComPerfilQuery>> ListarPorParceiroAsync(CancellationToken cancellationToken = default);
        Task<bool> ExisteUsuarioAsync(int usuarioId, Guid parceiroId, CancellationToken cancellationToken = default);
        Task IncrementarTentativaLoginAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task ResetarTentativasLoginAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task BloquearUsuarioAsync(int UsuarioId, CancellationToken cancellationToken);
        Task<UsuarioQuery?> ObterPorIdAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(UsuarioCommand usuario, CancellationToken cancellationToken = default);
        Task<bool> VerificarUsuarioMasterAsyunc(string usuarioLogin, CancellationToken cancellationToken = default);
    }
}









