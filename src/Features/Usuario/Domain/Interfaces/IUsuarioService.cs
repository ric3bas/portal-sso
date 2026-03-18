namespace Portal.Features.Usuario.Domain.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarComPerfisAsync(CancellationToken cancellationToken = default);
        Task RegisterAsync(RegisterRequest request);
        Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(CancellationToken cancellationToken = default);
    }
}
