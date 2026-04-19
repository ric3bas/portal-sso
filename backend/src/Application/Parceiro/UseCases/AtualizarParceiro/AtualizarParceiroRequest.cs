namespace Portal.Application.Parceiro.UseCases.AtualizarParceiro;

public class AtualizarParceiroRequest
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
