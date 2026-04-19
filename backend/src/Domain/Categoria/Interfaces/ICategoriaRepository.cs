using Portal.Domain.Categoria;

namespace Portal.Domain.Categoria.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<CategoriaQuery>> ObterTodasAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaQuery>> ObterPorParceiroAsync(Guid parceiroId, CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaQuery>> ObterPorFiltroAsync(string nome, CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaQuery>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, CancellationToken cancellationToken);
        Task<CategoriaQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CategoriaQuery?> ObterPorIdEParceiroAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(CategoriaCommand categoria, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(CategoriaCommand categoria, CancellationToken cancellationToken);
        Task<int> InativarAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteNomeAsync(string nome, Guid parceiroId, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    }
}
