using Portal.Domain.Common;

namespace Portal.Application.Escopo.UseCases.ObterEscopos;

public class ObterEscoposRequest : PaginacaoFiltro
{
    public string? Nome { get; set; }
}
