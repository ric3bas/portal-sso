using Portal.Domain.Categoria;
using Portal.Domain.Common;

namespace Portal.Domain.Categoria.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<ResultadoPaginado<CategoriaQuery>> ObterTodasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
        Task<ResultadoPaginado<CategoriaQuery>> ObterPorParceiroAsync(Guid parceiroId, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
        Task<ResultadoPaginado<CategoriaQuery>> ObterPorFiltroAsync(string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
        Task<ResultadoPaginado<CategoriaQuery>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
        Task<CategoriaQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CategoriaQuery?> ObterPorIdEParceiroAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(CategoriaCommand categoria, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(CategoriaCommand categoria, CancellationToken cancellationToken);
        Task<int> InativarAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteNomeAsync(string nome, Guid parceiroId, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    }
}
