using Portal.Domain.Common;

namespace Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro;

public class ObterCategoriasPorFiltroRequest : PaginacaoFiltro
{
    public string? Nome { get; set; }
}
