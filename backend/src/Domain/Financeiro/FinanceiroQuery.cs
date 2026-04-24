using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;

namespace Portal.Domain.Financeiro;

public class FinanceiroQuery
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
    public Guid ParceiroId { get; set; }

    public TResponse ToResponse<TResponse>() where TResponse : ObterLancamentosFinanceirosResponse, new()
    {
        return new TResponse
        {
            Id = Id,
            LocacaoId = LocacaoId,
            ClienteId = ClienteId,
            ClienteNome = ClienteNome,
            EquipamentoId = EquipamentoId,
            EquipamentoNome = EquipamentoNome,
            DataRetirada = DataRetirada,
            DataDevolucao = DataDevolucao,
            DiasLocados = DiasLocados,
            ValorDiaria = ValorDiaria,
            ValorTotal = ValorTotal,
            DataLancamento = DataLancamento
        };
    }
}
