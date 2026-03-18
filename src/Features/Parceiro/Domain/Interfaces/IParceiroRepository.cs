 namespace Portal.Features.Parceiro.Domain.Interfaces {
    public interface IParceiroRepository {
        Task<IEnumerable<ParceiroResponse>> ObterTodosAsync(string? nome, CancellationToken cancellationToken = default);
        Task<ParceiroResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> InserirAsync(Portal.Domain.Entities.ParceiroEntity parceiro, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Portal.Domain.Entities.ParceiroEntity parceiro, CancellationToken cancellationToken = default);
        Task<ParceiroResponse?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
        Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default);
    }
 }
