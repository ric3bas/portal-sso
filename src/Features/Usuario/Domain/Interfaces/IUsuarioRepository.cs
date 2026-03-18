using Portal.Dominio.Entities;

namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> InserirAsync(UsuarioEntity usuario);
        Task<bool> ExisteLoginAsync(string login, Guid parceiroId, CancellationToken cancellationToken = default);
        Task<RegistroValidacao> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(Guid parceiroId, CancellationToken cancellationToken = default);
        Task<bool> ExisteUsuarioAsync(int usuarioId, Guid parceiroId, CancellationToken cancellationToken = default);
    }
}





