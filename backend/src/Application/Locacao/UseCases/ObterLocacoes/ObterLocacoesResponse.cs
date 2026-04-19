using Portal.Domain.Locacao;

namespace Portal.Application.Locacao.UseCases.ObterLocacoes;

public class ObterLocacoesResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public Guid EquipamentoId { get; set; }
    public string EquipamentoNome { get; set; } = string.Empty;
    public StatusLocacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public DateTime DataRetirada { get; set; }
    public DateTime PrevisaoDevolucao { get; set; }
    public DateTime? DataDevolucaoReal { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal? ValorTotal { get; set; }
    public int? DiasLocados { get; set; }
    public string? Observacao { get; set; }
}
