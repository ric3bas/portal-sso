using Portal.Features.Categoria.Domain;

namespace Portal.Features.Categoria.Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<CategoriaEntity>> ObterTodasAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaEntity>> ObterPorParceiroAsync(Guid parceiroId, CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaEntity>> ObterPorFiltroAsync(string nome, CancellationToken cancellationToken);
        Task<IEnumerable<CategoriaEntity>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, CancellationToken cancellationToken);
        Task<CategoriaEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CategoriaEntity?> ObterPorIdEParceiroAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(string nome, Guid parceiroId, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(Guid id, string nome, bool? ativo, Guid parceiroId, CancellationToken cancellationToken);
        Task<int> InativarAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteNomeAsync(string nome, Guid parceiroId, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    }
}