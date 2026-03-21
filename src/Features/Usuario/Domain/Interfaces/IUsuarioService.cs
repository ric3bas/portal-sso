namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarComPerfisAsync(CancellationToken cancellationToken = default);
        Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(CancellationToken cancellationToken = default);
        Task IncrementarTentativaLogin(int UsuarioId, CancellationToken cancellationToken);
        Task ResetarTentativasLogin(int UsuarioId, CancellationToken cancellationToken);
    }
}
