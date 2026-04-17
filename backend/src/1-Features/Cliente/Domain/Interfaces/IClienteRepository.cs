using Portal.Features.Cliente.Domain;

namespace Portal.Features.Cliente.Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<IEnumerable<ClienteEntity>> ObterTodosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<ClienteEntity>> ObterPorFiltroAsync(FiltroClienteRequest filtro, CancellationToken cancellationToken);
        Task<ClienteEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(ClienteRequest cliente, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(Guid id, AtualizarClienteRequest cliente, CancellationToken cancellationToken);
        Task<int> BloquearAsync(Guid id,CancellationToken cancellationToken);
        Task<int> DesbloquearAsync(Guid id,CancellationToken cancellationToken);
        Task<int> InativarAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteCpfAsync(string cpf, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
        Task<bool> ExisteEmailAsync(string email, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
        
        // Métodos para telefones e endereços
        Task<TelefoneEntity> ObterTelefoneAsync(Guid clienteId, CancellationToken cancellationToken);
        Task<EnderecoEntity> ObterEnderecoAsync(Guid clienteId, CancellationToken cancellationToken);
        Task AtualizarTelefoneAsync(Guid clienteId, TelefoneRequest telefone, CancellationToken cancellationToken);
        Task AtualizarEnderecoAsync(Guid clienteId, EnderecoRequest endereco, CancellationToken cancellationToken);
    }
}
