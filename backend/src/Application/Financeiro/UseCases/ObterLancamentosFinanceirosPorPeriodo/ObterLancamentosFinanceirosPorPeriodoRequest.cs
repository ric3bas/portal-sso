using Portal.Domain.Common;

namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;

public class ObterLancamentosFinanceirosPorPeriodoRequest : PaginacaoFiltro
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
