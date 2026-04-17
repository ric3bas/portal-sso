using Portal.Domain.Base;

namespace Portal.Features.Cliente.Domain.Interfaces
{
    public interface IClienteService
    {
        Task<Result<IEnumerable<ClienteResponse>>> ObterTodosClientesAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<ClienteResponse>>> ObterClientesPorFiltroAsync(FiltroClienteRequest filtro, CancellationToken cancellationToken);
        Task<Result<ClienteResponse>> ObterClienteAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> CriarClienteAsync(ClienteRequest cliente, CancellationToken cancellationToken);
        Task<Result<string>> AtualizarClienteAsync(AtualizarClienteRequest cliente, CancellationToken cancellationToken);
        Task<Result<string>> BloquearClienteAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> DesbloquearClienteAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> InativarClienteAsync(string? id, CancellationToken cancellationToken);
    }
}