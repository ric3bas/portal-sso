namespace Portal.Domain.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Guid> CriarAsync(T entity, CancellationToken cancellationToken);
    Task<int> AtualizarAsync(T entity, CancellationToken cancellationToken);
    Task<int> ExcluirAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
}
