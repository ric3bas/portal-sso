namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;

public class ObterLancamentosFinanceirosPorPeriodoRequest
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
