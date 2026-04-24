
namespace Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;

public class ObterLancamentosFinanceirosResponse
{
    public Guid Id { get; set; }
    public Guid LocacaoId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid EquipamentoId { get; set; }
    public string EquipamentoNome { get; set; } = string.Empty;
    public DateTime DataRetirada { get; set; }
    public DateTime DataDevolucao { get; set; }
    public int DiasLocados { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataLancamento { get; set; }
}
