using Portal.Domain.Common;

namespace Portal.Application.Parceiro.UseCases.ObterParceirosPorFiltro;

public class ObterParceirosPorFiltroRequest : PaginacaoFiltro
{
    public string? Nome { get; set; } = string.Empty;
}
