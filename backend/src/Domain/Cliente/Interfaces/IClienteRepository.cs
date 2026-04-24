using Portal.Domain.Cliente;
using Portal.Domain.Common;

namespace Portal.Domain.Cliente.Interfaces;

public interface IClienteRepository
{
    Task<ResultadoPaginado<ClienteQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<ResultadoPaginado<ClienteQuery>> ObterPorFiltroAsync(string? nome, string? cpf, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<ClienteQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CriarAsync(ClienteCommand cliente, CancellationToken cancellationToken);
    Task<int> AtualizarAsync(ClienteCommand cliente, CancellationToken cancellationToken);
    Task<int> DefinirBloqueadoAsync(Guid id, bool bloqueado, CancellationToken cancellationToken);
    Task<int> InativarAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteCpfAsync(string cpf, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
}
