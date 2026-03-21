using Portal.Dominio.Entities;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> InserirAsync(UsuarioEntity usuario, CancellationToken cancellationToken);
        Task<bool> ExisteLoginAsync(string login, Guid parceiroId, CancellationToken cancellationToken = default);
        Task<RegistroValidacao> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(Guid parceiroId, CancellationToken cancellationToken = default);
        Task<bool> ExisteUsuarioAsync(int usuarioId, Guid parceiroId, CancellationToken cancellationToken = default);
        Task IncrementarTentativaLoginAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task ResetarTentativasLoginAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task BloquearUsuarioAsync(int UsuarioId, CancellationToken cancellationToken);
        Task<UsuarioEntity?> ObterPorIdAsync(int UsuarioId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(UsuarioEntity usuario, CancellationToken cancellationToken = default);
    }
}









