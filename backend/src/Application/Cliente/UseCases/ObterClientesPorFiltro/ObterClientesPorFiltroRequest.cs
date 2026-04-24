using Portal.Domain.Common;

namespace Portal.Application.Cliente.UseCases.ObterClientesPorFiltro;

public class ObterClientesPorFiltroRequest : PaginacaoFiltro
{
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
}
