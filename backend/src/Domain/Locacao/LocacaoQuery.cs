using Portal.Application.Locacao.UseCases.ObterLocacoes;

namespace Portal.Domain.Locacao;

public class LocacaoQuery
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid EquipamentoId { get; set; }
    public string EquipamentoNome { get; set; } = string.Empty;
    public StatusLocacao Status { get; set; }
    public DateTime DataRetirada { get; set; }
    public DateTime PrevisaoDevolucao { get; set; }
    public DateTime? DataDevolucaoReal { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal? ValorTotal { get; set; }
    public int? DiasLocados { get; set; }
    public string? Observacao { get; set; }
    public Guid ParceiroId { get; set; }

    public TResponse ToResponse<TResponse>() where TResponse : ObterLocacoesResponse, new()
    {
        return new TResponse
        {
            Id = Id,
            ClienteId = ClienteId,
            ClienteNome = ClienteNome,
            EquipamentoId = EquipamentoId,
            EquipamentoNome = EquipamentoNome,
            Status = Status,
            StatusDescricao = Status.ToString(),
            DataRetirada = DataRetirada,
            PrevisaoDevolucao = PrevisaoDevolucao,
            DataDevolucaoReal = DataDevolucaoReal,
            ValorDiaria = ValorDiaria,
            ValorTotal = ValorTotal,
            DiasLocados = DiasLocados,
            Observacao = Observacao
        };
    }
}
