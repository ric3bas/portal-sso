namespace Portal.Domain.Financeiro;

public class FinanceiroCommand
{
    public Guid Id { get; set; }
    public Guid LocacaoId { get; set; }
    public DateTime DataDevolucao { get; set; }
}
