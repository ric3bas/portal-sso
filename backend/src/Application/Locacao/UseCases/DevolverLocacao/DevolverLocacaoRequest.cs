namespace Portal.Application.Locacao.UseCases.DevolverLocacao;

public class DevolverLocacaoRequest
{
    public Guid Id { get; set; }
    public DateTime DataDevolucao { get; set; }
    public string? Observacao { get; set; }
}
