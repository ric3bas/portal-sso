namespace Portal.Domain.Parceiro.Interfaces;

public interface IParceiroRepository
{
    Task<IEnumerable<ParceiroQuery>> ObterTodosAsync(Guid? id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ParceiroQuery>> ObterTodosPorFiltroAsync(string nome, CancellationToken cancellationToken = default);
    Task<ParceiroQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParceiroQuery?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<Guid> InserirAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default);
    Task AtualizarAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default);
    Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default);
}
