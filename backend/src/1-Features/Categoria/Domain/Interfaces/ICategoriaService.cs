using Portal.Domain.Base;

namespace Portal.Features.Categoria.Domain.Interfaces
{
    public interface ICategoriaService
    {
        Task<Result<IEnumerable<CategoriaResponse>>> ObterTodasCategoriasAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<CategoriaResponse>>> ObterCategoriasPorFiltroAsync(string? nome, CancellationToken cancellationToken);
        Task<Result<CategoriaResponse>> ObterCategoriaAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> CriarCategoriaAsync(CategoriaRequest categoria, CancellationToken cancellationToken);
        Task<Result<string>> AtualizarCategoriaAsync(AtualizarCategoriaRequest categoria, CancellationToken cancellationToken);
        Task<Result<string>> InativarCategoriaAsync(string? id, CancellationToken cancellationToken);
    }
}