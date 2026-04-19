using Portal.Domain.Usuario;

namespace Portal.Domain.Usuario.Interfaces;

public interface IUsuarioRepository
{
    Task<int> InserirAsync(UsuarioCommand usuario, CancellationToken cancellationToken);
    Task<(bool ParceiroExiste, bool LoginExiste, bool PerfilExiste)> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsuarioComPerfilQuery>> ListarPorParceiroAsync(Guid? parceiroId, CancellationToken cancellationToken = default);
    Task IncrementarTentativaLoginAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task ResetarTentativasLoginAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioQuery?> ObterPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task AtualizarAsync(UsuarioCommand usuario, CancellationToken cancellationToken = default);
}
