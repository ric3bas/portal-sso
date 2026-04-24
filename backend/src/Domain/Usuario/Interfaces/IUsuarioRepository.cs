 using Portal.Domain.Usuario;
using Portal.Domain.Common;

namespace Portal.Domain.Usuario.Interfaces;

public interface IUsuarioRepository
{
    Task<int> InserirAsync(UsuarioCommand usuario, CancellationToken cancellationToken);
    Task<(bool ParceiroExiste, bool LoginExiste, bool PerfilExiste)> ValidarRegistroAsync(string login, Guid parceiroId, int perfilId, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<UsuarioComPerfilQuery>> ObterPorParceiroAsync(Guid? parceiroId, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task IncrementarTentativaLoginAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task ResetarTentativasLoginAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioQuery?> ObterPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task AtualizarAsync(UsuarioCommand usuario, CancellationToken cancellationToken = default);
}

